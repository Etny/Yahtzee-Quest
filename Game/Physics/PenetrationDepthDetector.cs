﻿using GlmSharp;
using Silk.NET.OpenGL;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Game.Physics
{
    class PenetrationDepthDetector
    {
        private PhysicsManager _pm;

        private static float Error = .00001f;

        public PenetrationDepthDetector(PhysicsManager physicsManager)
        {
            this._pm = physicsManager;
        }

        /// <summary>
        /// Uses the result of GJK and runs EPA to find the smallest vector to move either entity by to have the objects making touching contact rather than penetration.
        /// </summary>
        /// <param name="result">The GJK result used for the calculation</param>
        /// <returns>The vector to move either entity by</returns>
        /// 
        public vec3 GetPenetrationDepth(CollisionResult result)
            => GetPenetrationInfo(result).Item2;

        public vec3 Contact(CollisionResult result)
        {
            if (!result.Colliding) return vec3.NaN;

            return Contact(GetPenetrationInfo(result));
        }

        public vec3 Contact((Triangle, vec3) info)
        {
            var tri = info.Item1;
            var depth = info.Item2;

            vec3 barycentric = tri.ProjectOrigin(depth);

            return tri.Points[0].A * barycentric.x 
                 + tri.Points[1].A * barycentric.y 
                 + tri.Points[2].A * barycentric.z;
        }

        public (vec3,vec3) ContactDouble((Triangle, vec3) info)
        {
            var tri = info.Item1;
            var depth = info.Item2;

            vec3 barycentric = tri.ProjectOrigin(depth);

            return (tri.Points[0].A * barycentric.x
                  + tri.Points[1].A * barycentric.y
                  + tri.Points[2].A * barycentric.z,

                    tri.Points[0].B * barycentric.x
                  + tri.Points[1].B * barycentric.y
                  + tri.Points[2].B * barycentric.z);
        }

        public (Triangle, vec3) GetPenetrationInfo(CollisionResult result)
        {
            if (!result.Colliding) return (null, vec3.Zero);

            var removed = new List<vec3>();

            List<Triangle> tris = new List<Triangle>
            {
                new Triangle(result.Simplex, 0, 1, 2),
                new Triangle(result.Simplex, 0, 1, 3),
                new Triangle(result.Simplex, 0, 2, 3),
                new Triangle(result.Simplex, 1, 2, 3)
            };

            //Check if we're making touching contact and return early if we are
            foreach (Triangle t in tris)
                if (t.DistToOrigin() <= Error && t.IsClosestPointOnTriangle())
                    return (t, t.ClosestPoint());

            for (int i = 0; i < 999; i++)
            {
                //Remove invalid tris
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);

                //Find triangle closest to origin
                Triangle closestTri = null;

                foreach (Triangle tri in tris)
                    if ((closestTri == null || tri.ClosestPoint().LengthSqr < closestTri.ClosestPoint().LengthSqr) && tri.IsClosestPointOnTriangle())
                        closestTri = tri;

                SupportPoint newPoint = _pm.Collisions.SumSupport(result.M1.Collision, result.M2.Collision, closestTri.Normal);

                //Return the closest point on the tri if it lies on the support plane
                if(closestTri.OnSupportPlane(newPoint.Sup))
                    return (closestTri, closestTri.ClosestPoint());

                //Return the current closest triangle if we're stuck in an oscillating loop
                if (removed.Contains(newPoint.Sup))
                    return (closestTri, closestTri.ClosestPoint());

                //Return the new point if the distance delta gets bellow a small error
                if (newPoint.Sup.Length - closestTri.ClosestPoint().Length <= Error)
                    return (closestTri, newPoint.Sup); 

                //Find all tris that are facing the new point
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, newPoint.Sup) > 0);

                foreach (Triangle t in facingNewPoint)
                {
                    //Remove all those triangles
                    tris.Remove(t);
                    removed.AddRange(t.Vec3Points);

                    //Construct a new triangle {A, B, newPoint} for each edge AB of
                    //the tri that is unique among the triangles facing the new point
                    for (int j = 0; j < 3; j++)
                    {
                        var A = t.Points[j];
                        var B = t.Points[(j + 1) % 3];


                        if (facingNewPoint.Exists(tOther => tOther != t && 
                                                            tOther.Vec3Points.Contains(A.Sup) && 
                                                            tOther.Vec3Points.Contains(B.Sup) )) continue;

                        Triangle newTri = new Triangle(A, B, newPoint);
                        foreach (vec3 p in newTri.Vec3Points) removed.Remove(p);
                        tris.Add(newTri);
                    }
                }

            }

            Console.WriteLine("Penetration Depth testing exceed max steps!");


            return (null, vec3.NaN);
        }



#if DEBUG
            //Used for debugging
        public vec3 GetPenetrationDepthStep(CollisionResult result, ref List<Triangle> tris, ref List<vec3> removed, ref Triangle closestTri, ref SupportPoint newPoint, ref int counter)
        {
            //if (!result.Colliding) return vec3.Zero;

            if (counter == 0)
            {
                tris = new List<Triangle>
                {
                    new Triangle(result.Simplex, 0, 1, 2),
                    new Triangle(result.Simplex, 0, 1, 3),
                    new Triangle(result.Simplex, 0, 2, 3),
                    new Triangle(result.Simplex, 1, 2, 3)
                };

                foreach (Triangle t in tris)
                    if (t.DistToOrigin() <= Error && t.IsClosestPointOnTriangle())
                        return t.ClosestPoint();
            }
            else if (counter == 1)
            {
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);
                
                closestTri = null;

                List<Triangle> tempTri = new List<Triangle>();

                foreach (Triangle tri in tris) {
                    if (closestTri == null || (tri.ClosestPoint().LengthSqr < closestTri.ClosestPoint().LengthSqr && tri.IsClosestPointOnTriangle()))
                        closestTri = tri;
                }

                
            }
            else if (counter == 2)
            {
                newPoint = _pm.Collisions.SumSupport(result.M1.Collision, result.M2.Collision,
                    closestTri.ClosestPoint().LengthSqr < Error ? closestTri.Normal : closestTri.Normal);

                if (removed.Contains(newPoint.Sup)) return closestTri.ClosestPoint();
            }
            else if (counter == 3)
            {
                if (newPoint.Sup.Length - closestTri.ClosestPoint().Length <= Error) return newPoint.Sup;

                if (closestTri.OnSupportPlane(newPoint.Sup))
                {
                    return closestTri.ClosestPoint();
                }


                
                var tempPoint = newPoint;
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, tempPoint.Sup) >= 0);

                foreach (Triangle t in facingNewPoint)
                {
                    //Remove all those triangles
                    tris.Remove(t);
                    removed.AddRange(t.Vec3Points);

                    //Construct a new triangle {A, B, newPoint} for each edge AB of
                    //the tri that is unique among the triangles facing the new point
                    for (int j = 0; j < 3; j++)
                    {
                        var A = t.Points[j];
                        var B = t.Points[(j + 1) % 3];


                        if (facingNewPoint.Exists(tOther => tOther != t &&
                                                            tOther.Vec3Points.Contains(A.Sup) &&
                                                            tOther.Vec3Points.Contains(B.Sup))) continue;

                        var newTri = new Triangle(A, B, newPoint);
                        foreach (vec3 p in newTri.Vec3Points) removed.Remove(p);
                        //if (newTri.OnSupportPlane(newPoint.Sup)) return newTri.ClosestPoint();
                        tris.Add(newTri);
                    }

                }

                //closestTri.Subdivide(tris, newPoint, result, _pm.Collisions);
            }

            counter = counter <= 2 ? counter + 1 : 1;

            return vec3.NaN;
        }
#endif

        public class Triangle
        {
            public SupportPoint[] Points;
            public vec3[] Vec3Points { get { return Points.Select(p => p.Sup).ToArray(); } }

            private vec3 closest = vec3.NaN;
            private vec3 barycentric = vec3.NaN;
            private float? dist = null;

            public vec3 Normal { get { return vec3.Cross(Points[1].Sup - Points[0].Sup, Points[2].Sup - Points[0].Sup).Normalized; } }

            public vec3 Center { get { return Points[0].Sup / 3 + Points[1].Sup / 3 + Points[2].Sup / 3; } }

            public Triangle(SupportPoint A, SupportPoint B, SupportPoint C) 
            {
                //Ensure the normal is facing outwards
                if(vec3.Dot(vec3.Cross(B.Sup-A.Sup, C.Sup-A.Sup), A.Sup) <= 0)
                    Points = new SupportPoint[] { A, C, B };
                else
                    Points = new SupportPoint[] { A, B, C };
            }
            public Triangle(List<SupportPoint> list, int indexA, int indexB, int IndexC) : this(list[indexA], list[indexB], list[IndexC]) { }

            ///<summary>
            ///Returns the closest point to the origin on the triangles affine hull.
            ///As triangles don't move, the result is cached and returned on subsequent calls.
            ///</summary>
            ///<returns>The closest point to the origin on the triagnles affine hull</returns>
            public vec3 ClosestPoint()
            {
                if (closest != vec3.NaN) return closest;

                vec3 A = Points[0].Sup, B = Points[1].Sup, C = Points[2].Sup;

                mat3 orthoMatrix = new mat3(1, vec3.Dot(A, B - A), vec3.Dot(A, C - A),
                                            1, vec3.Dot(B, B - A), vec3.Dot(B, C - A),
                                            1, vec3.Dot(C, B - A), vec3.Dot(C, C - A));

                barycentric = orthoMatrix.Inverse.Column0;

                if (barycentric == vec3.NaN) return vec3.NaN;

                closest = (A * barycentric.x) + (B * barycentric.y) + (C * barycentric.z);

                return closest;
            }

            public bool IsClosestPointOnTriangle()
            {
                if (barycentric == vec3.NaN) ClosestPoint();

                return barycentric.MinElement >= 0;
            }

            public float DistToOrigin()
            {
                if(!dist.HasValue)
                    dist = vec3.Dot(Normal, Points[0].Sup);

                return dist.Value;
            }

            public bool OnSupportPlane(vec3 newPoint)
                => Vec3Points.Contains(newPoint) || Array.TrueForAll(Points, p => Math.Abs(vec3.Dot(Normal, (newPoint - p.Sup).Normalized)) <= Error);

            public bool OnSupportPlane(CollisionResult result, CollisionDetector coll)
                => OnSupportPlane(coll.SumSupport(result, ClosestPoint()).Sup);

            //Apapted from http://hacktank.net/blog/?p=119
            public vec3 ProjectOrigin(vec3 penDepth)
            {
                vec3 A = Points[0].Sup, B = Points[1].Sup, C = Points[2].Sup;
                vec3 AB = B - A, AC = C - A, AP = penDepth - A;

                float ABdAB = vec3.Dot(AB, AB);
                float ABdAC = vec3.Dot(AB, AC);
                float ACdAC = vec3.Dot(AC, AC);
                float APdAB = vec3.Dot(AP, AB);
                float APdAC = vec3.Dot(AP, AC);

                float denom = ABdAB * ACdAC - ABdAC * ABdAC;
                float v = (ACdAC * APdAB - ABdAC * APdAC) / denom;
                float w = (ABdAB * APdAC - ABdAC * APdAB) / denom;
                float u = 1.0f - v - w;

                return new vec3(u, v, w);
            }
        }
    }
}
