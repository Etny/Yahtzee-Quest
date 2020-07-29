using GlmSharp;
using Silk.NET.OpenGL;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Yahtzee.Game.Physics
{
    class PenetrationDepthDetector
    {
        private PhysicsManager physicsManager;

        private static float Error = .0001f;

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



                vec3 newPoint;


                if (closestTri.ClosestPoint().LengthSqr < Error)
                    newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.Normal);
                else
                    newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.ClosestPoint());

                if (vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[0]) == 0 
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[1]) == 0
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[2]) == 0)
                {
                    return closestTri.ClosestPoint();
                }

                if (newPoint.Length - length <= Error) { Console.WriteLine("Steps: " + i);  return newPoint; }

                Console.WriteLine($"Length on step {i}: {length}, with difference of {newPoint.Length - length} and new point {newPoint}");
                Console.WriteLine(tris.Count);
                //closestTri.Subdivide(tris, newPoint, result, physicsManager.Collisions);
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, newPoint) > 0);

                Console.WriteLine("Facing: " + facingNewPoint.Count);

                foreach (Triangle t in facingNewPoint)
                {
                    tris.Remove(t);
                    for (int j = 0; j < 3; j++)
                    {
                        vec3[] edge = new vec3[] { t.Points[j], t.Points[(j + 1) % 3] };

                        if (facingNewPoint.FindAll(tOther => tOther != t).Exists(tOther => tOther.Points.Contains(edge[0]) && tOther.Points.Contains(edge[1]))) continue;

                        tris.Add(new Triangle(edge[0], edge[1], newPoint));
                    }
                }

            }

            Console.WriteLine("Penetration Depth testing exceed max steps!");

            return vec3.NaN;
        }

        public vec3 GetPenetrationDepthStep(CollisionResult result, ref List<Triangle> tris, ref Triangle closestTri, ref vec3 newPoint, ref int counter)
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
                //Console.WriteLine(closestTri.ClosestPoint().LengthSqr + " " + float.Epsilon * 10);
                if (closestTri.ClosestPoint().LengthSqr < Error)
                    newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.Normal);
                else
                    newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2, closestTri.ClosestPoint());
                
            }
            else if (counter == 3)
            {
                if (newPoint.Length - closestTri.ClosestPoint().Length <= Error) return newPoint;
                //closestTri.Subdivide(tris, newPoint, result, physicsManager.Collisions);

                if (vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[0]) == 0
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[1]) == 0
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[2]) == 0)
                {
                    return closestTri.ClosestPoint();
                }

                vec3 tempPoint = newPoint;
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, tempPoint) > 0);

                Console.WriteLine("Facing: " + facingNewPoint.Count);

                foreach(Triangle t in facingNewPoint)
                {
                    tris.Remove(t);
                    for(int i = 0; i < 3; i++)
                    {
                        vec3[] edge = new vec3[] { t.Points[i], t.Points[(i + 1) % 3] };

                        if (facingNewPoint.FindAll(tOther => tOther != t).Exists(tOther => tOther.Points.Contains(edge[0]) && tOther.Points.Contains(edge[1]))) continue;

                        tris.Add(new Triangle(edge[0], edge[1], newPoint));
                    }
                }
            }


            counter = counter <= 2 ? counter + 1 : 1;

            return vec3.NaN;
        }


        public class Triangle
        {
            public vec3[] Points;

            private vec3 closest = vec3.NaN;

            public vec3 Normal { get { return vec3.Cross(Points[1] - Points[0], Points[2] - Points[0]); } }

            public vec3 Center { get { return Points[0] / 3 + Points[1] / 3 + Points[2] / 3; } }

            public Triangle(vec3 A, vec3 B, vec3 C) 
            {
                if(vec3.Dot(vec3.Cross(B-A, C-A), A) <= 0)
                    Points = new vec3[] { A, C, B };
                else
                {
                    //Console.WriteLine("Needed to rewind >:(");
                    Points = new vec3[] { A, B, C };
                }


            }
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

                if (result == vec3.NaN /*|| result.MinElement < 0*/) return vec3.NaN;

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
