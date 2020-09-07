using GlmSharp;
using Silk.NET.OpenGL;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Yahtzee.Main;
using db = System.Diagnostics.Debug;


namespace Yahtzee.Game.Physics
{
    class PenetrationDepthDetector
    {
        private PhysicsManager _pm;

        private static float AngleError = .000001f;

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
            => GetPenetrationInfo(result).Item1.ClosestPoint();

        public (vec3,vec3) GetContactInfo((Triangle, vec3) info)
        {
            var tri = info.Item1;
            var depth = info.Item1.ClosestPoint();

            vec3 barycentric = tri.ProjectOrigin(depth);

            db.Assert(barycentric != vec3.NaN);

            return (tri.Points[0].A * barycentric.x
                  + tri.Points[1].A * barycentric.y
                  + tri.Points[2].A * barycentric.z,

                    tri.Points[0].B * barycentric.x
                  + tri.Points[1].B * barycentric.y
                  + tri.Points[2].B * barycentric.z);
        }


        public (Triangle, vec3) GetPenetrationInfo(CollisionResult result)
        {
            if (!result.Colliding) return (null, vec3.NaN);

            vec3 center = (result.Simplex[0].Sup / 4) + (result.Simplex[1].Sup / 4) + (result.Simplex[2].Sup / 4) + (result.Simplex[3].Sup / 4);

            List<Triangle> tris = new List<Triangle>
            {
                new Triangle(result.Simplex, 0, 1, 2, center),
                new Triangle(result.Simplex, 0, 1, 3, center),
                new Triangle(result.Simplex, 0, 2, 3, center),
                new Triangle(result.Simplex, 1, 2, 3, center)
            };

            db.Assert(result.M1.Transform.Translation != vec3.NaN);
            db.Assert(result.M2.Transform.Translation != vec3.NaN);

            for (int i = 0; i < 999; i++)
            {
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);

                Triangle closestTri = null;

                foreach (var t in tris)
                    if (closestTri == null || t.DistToOrigin() < closestTri.DistToOrigin())
                        closestTri = t;

                db.Assert(closestTri != null);

                var newPoint = _pm.Collisions.SumSupport(result, closestTri.Normal);

                if (closestTri.OnSupportPlane(newPoint.Sup))
                    return (closestTri, newPoint.Sup);

                if (tris.Exists(t => t.Vec3Points.Contains(newPoint.Sup)))
                    return (closestTri, newPoint.Sup);

                //else
                //    foreach (vec3 p in closestTri.Vec3Points) Console.WriteLine("Error: " + Math.Abs(vec3.Dot(closestTri.Normal, (newPoint.Sup - p).NormalizedSafe)));
                

                List<Triangle> trisToRemove = tris.FindAll(t => vec3.Dot(t.Normal, newPoint.Sup - t.Vec3Points[0]) > 0);

                foreach (var t in trisToRemove)
                {
                    tris.Remove(t);

                    for (int j = 0; j < 3; j++)
                    {
                        var A = t.Points[j];
                        var B = t.Points[(j + 1) % 3];


                        if (trisToRemove.Exists(tOther => tOther != t &&
                                                            tOther.Vec3Points.Contains(A.Sup) &&
                                                            tOther.Vec3Points.Contains(B.Sup))) continue;

                        Triangle newTri = new Triangle(A, B, newPoint, center);
                        if (tris.Exists(t => Array.TrueForAll(newTri.Vec3Points, p => t.Vec3Points.Contains(p)))) continue;
                        tris.Add(newTri);
                    }
                }
            }

            db.Assert(false);

            foreach (vec3 v in result.SimplexPos) Console.WriteLine("Failed on: " + v);
            Console.WriteLine("Pos: " + result.M1.Transform.Translation + ", Rot: " + result.M1.Transform.Rotation);

            //GetPenetrationInfo(result);

            return (null, vec3.NaN);
        }



#if DEBUG
            //Used for debugging
        public vec3 GetPenetrationDepthStep(CollisionResult result, ref List<Triangle> tris, ref List<vec3> removed, ref Triangle closestTri, ref SupportPoint newPoint, ref int counter)
        {
            //if (!result.Colliding) return vec3.Zero;

            if (counter == 0)
            {
                vec3 center = result.Simplex[0].Sup / 4 + result.Simplex[1].Sup / 4 + result.Simplex[2].Sup / 4 + result.Simplex[3].Sup / 4;

                tris = new List<Triangle>
                {
                    new Triangle(result.Simplex, 0, 1, 2, center),
                    new Triangle(result.Simplex, 0, 1, 3, center),
                    new Triangle(result.Simplex, 0, 2, 3, center),
                    new Triangle(result.Simplex, 1, 2, 3, center)
                };

                foreach (vec3 v in result.SimplexPos) Console.WriteLine("Showing: " + v);
                Console.WriteLine("Pos: " + result.M1.Transform.Translation + ", Rot: " + result.M1.Transform.Rotation);


            }
            else if (counter == 1)
            {
                tris = tris.FindAll(t => t.ClosestPoint() != vec3.NaN);

                closestTri = null;

                foreach (var t in tris)
                    if (closestTri == null || t.DistToOrigin() < closestTri.DistToOrigin())
                        closestTri = t;
            }
            else if (counter == 2)
            {
                newPoint = _pm.Collisions.SumSupport(result, closestTri.Normal);

                if (closestTri.OnSupportPlane(newPoint.Sup))
                    return newPoint.Sup;
                else
                    foreach(vec3 p in closestTri.Vec3Points) Console.WriteLine("Error: " + (Math.Acos(vec3.Dot(closestTri.Normal, (newPoint.Sup - p).NormalizedSafe)) - Util.ToRad(90)));

                var tempPoint = newPoint;
                if (tris.Exists(t => t.Vec3Points.Contains(tempPoint.Sup)))
                    return newPoint.Sup;

            }
            else if (counter == 3)
            {
                
                var tempPoint = newPoint;
                List<Triangle> facingNewPoint = tris.FindAll(t => vec3.Dot(t.Normal, tempPoint.Sup - t.Vec3Points[0]) > 0);

                //db.Assert(facingNewPoint.Contains(closestTri));

                foreach (Triangle t in facingNewPoint)
                {
                    tris.Remove(t);

                    for (int j = 0; j < 3; j++)
                    {
                        var A = t.Points[j];
                        var B = t.Points[(j + 1) % 3];


                        if (facingNewPoint.Exists(tOther => tOther != t &&
                                                            tOther.Vec3Points.Contains(A.Sup) &&
                                                            tOther.Vec3Points.Contains(B.Sup))) continue;

                        var newTri = new Triangle(A, B, newPoint);
                        if (tris.Exists(t => Array.TrueForAll(newTri.Vec3Points, p => t.Vec3Points.Contains(p)))) continue;
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

            public Triangle(SupportPoint A, SupportPoint B, SupportPoint C, vec3 center) 
            {
                //Ensure the normal is facing outwards
                if(vec3.Dot(vec3.Cross(B.Sup - A.Sup, C.Sup - A.Sup), A.Sup - center) <= 0)
                    Points = new SupportPoint[] { A, C, B };
                else
                    Points = new SupportPoint[] { A, B, C };
            }

            public Triangle(SupportPoint A, SupportPoint B, SupportPoint C) : this(A, B, C, vec3.Zero) { }

            public Triangle(List<SupportPoint> list, int indexA, int indexB, int IndexC, vec3 center) : this(list[indexA], list[indexB], list[IndexC], center) { }
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

            public float DistToOrigin()
            {
                if(!dist.HasValue)
                    dist = vec3.Dot(Normal, Points[0].Sup);

                return dist.Value;
            }

            public bool OnSupportPlane(vec3 newPoint)
                => Vec3Points.Contains(newPoint) || Array.TrueForAll(Points, p => Math.Abs(vec3.Dot(Normal, (newPoint - p.Sup).NormalizedSafe)) <= AngleError);

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

                db.Assert(new vec3(u, v, w) != vec3.NaN);

                return new vec3(u, v, w);
            }
        }
    }
}
