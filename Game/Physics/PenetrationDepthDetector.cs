using GlmSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Yahtzee.Game.Physics
{
    class PenetrationDepthDetector
    {
        private PhysicsManager physicsManager;

        private static float Error = .2f;//2.38418579E-07f;

        public PenetrationDepthDetector(PhysicsManager physicsManager)
        {
            this.physicsManager = physicsManager;
        }

        public vec3 GetPenetrationDepth(CollisionResult result)
        {
            if (!result.Colliding) return vec3.Zero;

            List<Triangle> tris = new List<Triangle>
            {
                new Triangle(result.Simplex, 0, 1, 2),
                new Triangle(result.Simplex, 0, 1, 3),
                new Triangle(result.Simplex, 0, 2, 3),
                new Triangle(result.Simplex, 1, 2, 3)
            };

            for (int i = 0; i < 999; i++)
            {
                Triangle closestTri = null;

                foreach (Triangle tri in tris)
                {
                    vec3 closest = tri.ClosestPoint();

                    if (closest == vec3.NaN) continue;

                    if (closestTri == null || closest.LengthSqr < closestTri.ClosestPoint().LengthSqr)
                        closestTri = tri;
                }
                float length = closestTri.ClosestPoint().Length;

                vec3 newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.ClosestPoint());

                if (newPoint.Length - length <= Error) { Console.WriteLine("Steps: " + i);  return newPoint; }

                Console.WriteLine($"Length on step {i}: {length}, with difference of {newPoint.Length - length} and new point {newPoint}");
                Console.WriteLine(tris.Count);
                closestTri.Subdivide(tris, newPoint, result, physicsManager.Collisions);
            }

            Console.WriteLine("Penetration Depth testing exceed max steps!");

            return vec3.NaN;
        }

        public vec3 GetPenetrationDepthStep(CollisionResult result, ref List<Triangle> tris, ref Triangle closestTri, ref vec3 newPoint, ref List<vec3> newEdgePoints, ref int counter)
        {
            if (!result.Colliding) return vec3.Zero;

            if (counter == 0)
            {
                tris = new List<Triangle>
                {
                    new Triangle(result.Simplex, 0, 1, 2),
                    new Triangle(result.Simplex, 0, 1, 3),
                    new Triangle(result.Simplex, 0, 2, 3),
                    new Triangle(result.Simplex, 1, 2, 3)
                };
            }
            else if (counter == 1)
            {
                closestTri = null;

                foreach (Triangle tri in tris)
                {
                    vec3 closest = tri.ClosestPoint();

                    if (closest == vec3.NaN) continue;

                    if (closestTri == null || closest.LengthSqr < closestTri.ClosestPoint().LengthSqr)
                        closestTri = tri;
                }
            }
            else if (counter == 2)
            {
                newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.ClosestPoint());
            }else if(counter == 3)
            {
                newEdgePoints.Add(closestTri.ClosestToOrigin(closestTri.Points[0], closestTri.Points[1]));
                newEdgePoints.Add(closestTri.ClosestToOrigin(closestTri.Points[1], closestTri.Points[2]));
                newEdgePoints.Add(closestTri.ClosestToOrigin(closestTri.Points[2], closestTri.Points[0]));
            }
            else if (counter == 4)
            {
                if (newPoint.Length - closestTri.ClosestPoint().Length <= Error) return newPoint;
                closestTri.Subdivide(tris, newPoint, result, physicsManager.Collisions);
            }


            counter = counter <= 3 ? counter + 1 : 1;

            return vec3.NaN;
        }


        public class Triangle
        {
            public vec3[] Points;

            private vec3 closest = vec3.NaN;

            public Triangle(vec3 A, vec3 B, vec3 C) { Points = new vec3[] { A, B, C }; }
            public Triangle(List<vec3> list, int indexA, int indexB, int IndexC) : this(list[indexA], list[indexB], list[IndexC]) { }

            public vec3 ClosestPoint()
            {
                if (closest != vec3.NaN) return closest;

                mat3 orthoMatrix = new mat3();

                for (int i = 0; i < 9; i++)
                {
                    if (i % 3 == 0) { orthoMatrix[i] = 1; continue; }
                    orthoMatrix[i] = vec3.Dot(Points[i / 3], (Points[i % 3] - Points[0]));
                }

                var result = orthoMatrix.Inverse.Column0;

                if (result.x < 0) result.x = 0;
                if (result.y < 0) result.y = 0;
                if (result.z < 0) result.z = 0;

                //if (result == vec3.NaN || result.MinElement < 0) return vec3.NaN;

                closest = (Points[0] * result.x) + (Points[1] * result.y) + (Points[2] * result.z);

                return closest;
            }

            public void Subdivide(List<Triangle> tris, vec3 newPoint, CollisionResult result, CollisionDetector coll)
            {
                tris.Remove(this);

                for (int i = 0; i < 3; i++)
                {
                    SplitEdge(tris, newPoint, result, coll, Points[i], Points[(i + 1) % 3]);
                }
            }

            private void SplitEdge(List<Triangle> tris, vec3 newPoint, CollisionResult result, CollisionDetector coll, vec3 A, vec3 B)
            {
                vec3 closestOnEdge = ClosestToOrigin(A, B);
                //if (closestOnEdge == closest) return;

                vec3 newPointOnEdge = coll.SumSupport(result.M1, result.M2, closestOnEdge);

                if (vec3.Dot(closestOnEdge, newPointOnEdge) == closestOnEdge.LengthSqr)
                {
                    if(newPoint != A && newPoint != B)
                        tris.Add(new Triangle(A, B, newPoint));
                }
                else
                {
                    if (newPointOnEdge != A && newPoint != A) tris.Add(new Triangle(A, newPointOnEdge, newPoint));
                    if (newPointOnEdge != B && newPoint != B) tris.Add(new Triangle(B, newPointOnEdge, newPoint));
                }
            }

            public vec3 ClosestToOrigin(vec3 A, vec3 B)
            {
                mat2 orthoMatrix = new mat2(1, vec3.Dot(A, (B - A)), 1, vec3.Dot(B, (B - A)));

                var result = (orthoMatrix.Inverse * mat2.Identity).Column0;

                if (result.y < 0) return A;
                if (result.x < 0) return B;

                return (A * result.x) + (B * result.y);
            }
        }
    }
}
