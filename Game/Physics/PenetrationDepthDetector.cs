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

        private static float Error = 1E-10f;

        public PenetrationDepthDetector(PhysicsManager physicsManager)
        {
            this.physicsManager = physicsManager;
        }

        /// <summary>
        /// Uses the result of GJK to find the smallest vector to move either entity by to have the objects making touching contact rather than penetration.
        /// </summary>
        /// <param name="result">The GJK result used for the calculation</param>
        /// <returns>The vector to move either entity by</returns>
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
                //Remove invalid tris
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);

                //Find triangle closest to origin
                Triangle closestTri = null;

                foreach (Triangle tri in tris)
                    if (closestTri == null || tri.ClosestPoint().LengthSqr < closestTri.ClosestPoint().LengthSqr)
                        closestTri = tri;

                //If the origin lies within the tri, use its normal as the search direction
                vec3 newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2,
                    closestTri.ClosestPoint().LengthSqr < Error ? closestTri.Normal : closestTri.ClosestPoint());

                //Return the closest point on the tri if it lies on the support plane
                if(closestTri.Points.Sum(p => vec3.Dot(closestTri.Normal, newPoint - p)) == 0)
                    return closestTri.ClosestPoint();

                //Return the new point if the distance delta gets bellow a small error
                if (newPoint.Length - closestTri.ClosestPoint().Length <= Error)
                    return newPoint; 

                //Find all tris that are facing the new point
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, newPoint) > 0);

                foreach (Triangle t in facingNewPoint)
                {
                    //Remove all those triangles
                    tris.Remove(t);

                    //Construct a new triangle {A, B, newPoint} for each edge AB of
                    //the tri that is unique among the triangles facing the new point
                    for (int j = 0; j < 3; j++)
                    {
                        vec3 A = t.Points[j];
                        vec3 B = t.Points[(j + 1) % 3];


                        if (facingNewPoint.Exists(tOther => tOther != t && 
                                                            tOther.Points.Contains(A) && 
                                                            tOther.Points.Contains(B) )) continue;

                        tris.Add(new Triangle(A, B, newPoint));
                    }
                }

            }

            Console.WriteLine("Penetration Depth testing exceed max steps!");

            return vec3.NaN;
        }

#if DEBUG
        //Used for debugging
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
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);
                
                closestTri = null;

                foreach (Triangle tri in tris)
                    if (closestTri == null || tri.ClosestPoint().LengthSqr < closestTri.ClosestPoint().LengthSqr)
                        closestTri = tri;
            }
            else if (counter == 2)
            {
                newPoint = physicsManager.Collisions.SumSupport(result.M1, result.M2,
                    closestTri.ClosestPoint().LengthSqr < Error ? closestTri.Normal : closestTri.ClosestPoint());
            }
            else if (counter == 3)
            {
                if (newPoint.Length - closestTri.ClosestPoint().Length <= Error) return newPoint;

                if (vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[0]) == 0
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[1]) == 0
                    && vec3.Dot(closestTri.Normal, newPoint - closestTri.Points[2]) == 0)
                {
                    return closestTri.ClosestPoint();
                }

                vec3 tempPoint = newPoint;
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, tempPoint) > 0);

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
#endif

        public class Triangle
        {
            public vec3[] Points;

            private vec3 closest = vec3.NaN;

            public vec3 Normal { get { return vec3.Cross(Points[1] - Points[0], Points[2] - Points[0]); } }

            public vec3 Center { get { return Points[0] / 3 + Points[1] / 3 + Points[2] / 3; } }

            public Triangle(vec3 A, vec3 B, vec3 C) 
            {
                //Ensure the normal is facing outwards
                if(vec3.Dot(vec3.Cross(B-A, C-A), A) <= 0)
                    Points = new vec3[] { A, C, B };
                else
                    Points = new vec3[] { A, B, C };
            }
            public Triangle(List<vec3> list, int indexA, int indexB, int IndexC) : this(list[indexA], list[indexB], list[IndexC]) { }

            ///<summary>Returns the closest point to the origin on the triangles affine hull</summary>
            ///<returns>The closest point to the origin on the triagnles affine hull</returns>
            public vec3 ClosestPoint()
            {
                if (closest != vec3.NaN) return closest;

                vec3 A = Points[0], B = Points[1], C = Points[2];

                mat3 orthoMatrix = new mat3(1, vec3.Dot(A, B - A), vec3.Dot(A, C - A),
                                            1, vec3.Dot(B, B - A), vec3.Dot(B, C - A),
                                            1, vec3.Dot(C, B - A), vec3.Dot(C, C - A));

                var barycentric = orthoMatrix.Inverse.Column0;

                if (barycentric == vec3.NaN) return vec3.NaN;

                closest = (Points[0] * barycentric.x) + (Points[1] * barycentric.y) + (Points[2] * barycentric.z);

                return closest;
            }
        }
    }
}
