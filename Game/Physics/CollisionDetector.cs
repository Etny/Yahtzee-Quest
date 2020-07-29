using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Silk.NET.Vulkan;

namespace Yahtzee.Game.Physics
{
    class CollisionDetector
    {
        private static float Error = 2.38418579E-07f;


        public bool GJK(ModelEntity m1, ModelEntity m2)
            => GJKResult(m1, m2).Colliding;

        public CollisionResult GJKResult(ModelEntity m1, ModelEntity m2)
        {
            List<vec3> simplex = new List<vec3> { SumSupport(m1, m2, vec3.UnitY) };
            vec3 Direction = -simplex[0];

            CollisionResult result = new CollisionResult(m1, m2, simplex);

            for (int i = 0; i < 1000; i++)
            {
                vec3 P = SumSupport(m1, m2, Direction);
                if (vec3.Dot(P.Normalized, Direction.Normalized) < 0) return result;
                simplex.Add(P);
                if (DoSimplex(simplex)) { result.Colliding = true; return result; }
                if (simplex.Find(x => x.Length <= Error).Length > 0 || Direction.Length <= Error) return result;
                Direction = GetNewDirection(simplex);
            }

            Console.WriteLine("GJK exceeded max steps!");

            return result;
        }

        // Used for debugging
        public int GJK_Step(ModelEntity m1, ModelEntity m2, List<vec3> simplex, ref vec3 Direction, ref int counter)
        {
            if (counter == 0)
            {
                simplex.Add(SumSupport(m1, m2, vec3.UnitY));
                Direction = -simplex[0];
            }

            vec3 P = SumSupport(m1, m2, Direction);

            if (counter == 1)
                simplex.Add(P);
            else if (counter == 2 && vec3.Dot(P.Normalized, Direction.Normalized) < 0) return 2;
            else if (counter == 2 && DoSimplex(simplex)) return 1;
            else if (counter == 3) Direction = GetNewDirection(simplex);

            counter++;
            if (counter >= 4) counter = 1;
            return 0;
        }

        public vec3 SumSupport(ModelEntity m1, ModelEntity m2, vec3 Dir)
            => SingleSupport(m1, Dir) - SingleSupport(m2, -Dir);

        public vec3 SingleSupport(ModelEntity m1, vec3 Dir)
        {
            vec3 p = m1.Transform.Apply(m1.collision.CollisionVertices[0]);
            float maxDot = 0;

            for (int i = 0; i < m1.collision.CollisionVertices.Length; i++)
            {
                vec3 v = m1.Transform.Apply(m1.collision.CollisionVertices[i]);
                float dot = vec3.Dot(Dir, v);
                if (dot <= maxDot && i != 0) continue;
                maxDot = dot;
                p = v;
            }

            return p;
        }

        // Used for debugging
        public int SingleSupportIndex(ModelEntity m1, vec3 Dir)
        {
            int index = -1;
            float maxDot = -9999;
            float td = -9999;
            float ti = -1;

            for (int i = 0; i < m1.collision.CollisionVertices.Length; i++)
            {
                vec3 v = m1.Transform.Apply(m1.collision.CollisionVertices[i]);
                float dot = vec3.Dot(Dir, v);
                if (v == new vec3(0.5f, -0.5f, 0.5f)) { td = dot; ti = i; }
                if (dot <= maxDot) continue;
                maxDot = dot;
                index = i;
            }

            return index;
        }


        private bool DoSimplex(List<vec3> simplex)
        {
            return simplex.Count switch
            {
                2 => SimplexLine(simplex),
                3 => SimplexTriangle(simplex),
                4 => SimplexTetraHedron(simplex),
                _ => false,
            };
        }

        private vec3 GetNewDirection(List<vec3> simplex)
        {
            switch (simplex.Count)
            {
                case 1:
                    return -simplex[0];

                case 2:
                    vec3 lineA = simplex[1];
                    vec3 lineB = simplex[0];
                    vec3 lineAB = lineB - lineA;

                    return vec3.Cross(vec3.Cross(lineAB, -lineA), lineAB);

                case 3:
                    vec3 A = simplex[2];
                    vec3 B = simplex[1];
                    vec3 C = simplex[0];
                    vec3 AB = B - A;
                    vec3 AC = C - A;
                    vec3 Normal = vec3.Cross(AB, AC);

                    if (vec3.Dot(Normal, C) <= 0)
                        return Normal;
                    else
                    {
                        vec3 temp = simplex[0];
                        simplex[0] = simplex[1];
                        simplex[1] = temp;
                        return -Normal;
                    }


                default:
                    return vec3.NaN;
            }
        }

        private bool SimplexLine(List<vec3> line)
        {
            vec3 A = line[1];
            vec3 B = line[0];

            vec3 AB = B - A;
            vec3 BA = A - B;

            // Check if origin lies beyond each vertex by comparing the 
            // vertex vector and vector from other the other vertex

            if (vec3.Dot(B, AB) <= 0) // Origin lies beyond B
                line.RemoveAt(1); // Simplex = {B}

            else if (vec3.Dot(A, BA) <= 0) // Origin lies beyond A
                line.RemoveAt(0); // Simplex = {A}

            // Else origin lies in region AB
            // Simplex = {A,B}

            return false;
        }

        private bool SimplexTriangle(List<vec3> tri)
        {
            vec3 A = tri[2];
            vec3 B = tri[1];
            vec3 C = tri[0];

            vec3 AB = B - A;
            vec3 AC = C - A;
            vec3 BA = A - B;
            vec3 BC = C - B;
            vec3 CA = A - C;
            vec3 CB = B - C;

            // Check if origin lies beyond each vertex by comparing the 
            // vertex vector and vectors from other vertices to that vertex

            if (vec3.Dot(A, BA) <= 0 && vec3.Dot(A, CA) <= 0)
            { // Origin lies beyond A
                tri.RemoveRange(0, 2); // Simplex = {A}
            }

            else if (vec3.Dot(B, AB) <= 0 && vec3.Dot(B, CB) <= 0) // Origin lies beyond B
            {
                tri.RemoveAt(2); // Simplex = {B}
                tri.RemoveAt(0);
            }

            else if (vec3.Dot(C, AC) <= 0 && vec3.Dot(C, BC) <= 0) // Origin lies beyond C
                tri.RemoveRange(1, 2); // Simplex = {C}

            else
            {
                // Check if origin lies within a face region by comparing the 
                // tri normal with the cross between the face vertices' vectors

                vec3 Normal = vec3.Cross(AB, AC);
                vec3 NormalAB = vec3.Cross(A, B);
                vec3 NormalAC = vec3.Cross(C, A);
                vec3 NormalBC = vec3.Cross(B, C);

                if (vec3.Dot(NormalAB, Normal) <= 0) // Origin lies in region AB 
                    tri.RemoveAt(0); // Simplex = {A,B}

                else if (vec3.Dot(NormalAC, Normal) <= 0) // Origin lies in region AC
                    tri.RemoveAt(1); // Simplex = {A,C}

                else if (vec3.Dot(NormalBC, Normal) <= 0) // Origin lies in region BC
                    tri.RemoveAt(2); // Simplex = {B,C}

                // Else origin lies within region ABC
                // Simplex = {A,B,C}
            }

            return false;
        }

        private bool SimplexTetraHedron(List<vec3> tetra)
        {
            vec3 A = tetra[3];
            vec3 B = tetra[2];
            vec3 C = tetra[1];
            vec3 D = tetra[0];

            vec3 AB = B - A;
            vec3 AC = C - A;
            vec3 AD = D - A;
            vec3 BA = A - B;
            vec3 BC = C - B;
            vec3 BD = D - B;
            vec3 CA = A - C;
            vec3 CB = B - C;
            vec3 CD = D - C;
            vec3 DA = A - D;
            vec3 DB = B - D;
            vec3 DC = C - D;

            // Check if origin lies beyond each vertex by comparing the 
            // vertex vector and vectors from other vertices to that vertex

            if (vec3.Dot(A, BA) <= 0 && vec3.Dot(A, CA) <= 0 && vec3.Dot(A, DA) <= 0) // Origin lies beyond A
                tetra.RemoveRange(0, 3); // Simplex = {A}

            else if (vec3.Dot(B, AB) <= 0 && vec3.Dot(B, CB) <= 0 && vec3.Dot(B, DB) <= 0) // Origin lies beyond B
            {
                tetra.RemoveAt(3); // Simplex = {B}
                tetra.RemoveRange(0, 2);
            }

            else if (vec3.Dot(C, AC) <= 0 && vec3.Dot(C, BC) <= 0 && vec3.Dot(C, DC) <= 0) // Origin lies beyond C
            {
                tetra.RemoveRange(2, 2); // Simplex = {C}
                tetra.RemoveAt(0);
            }

            else if (vec3.Dot(D, AD) <= 0 && vec3.Dot(D, BD) <= 0 && vec3.Dot(D, CD) <= 0) // Origin lies beyond D
                tetra.RemoveRange(1, 3); // Simplex = {D}

            else
            {
                vec3 NormalABC = vec3.Cross(AB, AC);
                float DotABC_AB = vec3.Dot(vec3.Cross(A, B), NormalABC);
                float DotABC_AC = vec3.Dot(vec3.Cross(C, A), NormalABC);
                float DotABC_BC = vec3.Dot(vec3.Cross(B, C), NormalABC);

                vec3 NormalABD = vec3.Cross(AD, AB);
                float DotABD_AB = vec3.Dot(vec3.Cross(B, A), NormalABD);
                float DotABD_AD = vec3.Dot(vec3.Cross(A, D), NormalABD);
                float DotABD_BD = vec3.Dot(vec3.Cross(D, B), NormalABD);

                vec3 NormalACD = vec3.Cross(AC, AD);
                float DotACD_AC = vec3.Dot(vec3.Cross(A, C), NormalACD);
                float DotACD_AD = vec3.Dot(vec3.Cross(D, A), NormalACD);
                float DotACD_CD = vec3.Dot(vec3.Cross(C, D), NormalACD);

                vec3 NormalBCD = vec3.Cross(CB, CD);
                float DotBCD_BC = vec3.Dot(vec3.Cross(C, B), NormalBCD);
                float DotBCD_BD = vec3.Dot(vec3.Cross(B, D), NormalBCD);
                float DotBCD_CD = vec3.Dot(vec3.Cross(D, C), NormalBCD);

                if (DotABC_AB <= 0 && DotABD_AB <= 0) // Origin lies in region AB
                    tetra.RemoveRange(0, 2); // Simplex = {A,B}

                else if (DotABC_AC <= 0 && DotACD_AC <= 0) // Origin lies in region AC
                {
                    tetra.RemoveAt(2); // Simplex = {A,C}
                    tetra.RemoveAt(0);
                }

                else if (DotABC_BC <= 0 && DotBCD_BC <= 0) // Origin lies in region BC
                {
                    tetra.RemoveAt(3); // Simplex = {B,C}
                    tetra.RemoveAt(0);
                }

                else if (DotABD_AD <= 0 && DotACD_AD <= 0) // Origin lies in region AD
                    tetra.RemoveRange(1, 2); // Simplex = {A,D}

                else if (DotABD_BD <= 0 && DotBCD_BD <= 0) // Origin lies in region BD
                {
                    tetra.RemoveAt(3); // Simplex = {B,D}
                    tetra.RemoveAt(1);
                }

                else if (DotACD_CD <= 0 && DotBCD_CD <= 0) // Origin lies in region CD
                    tetra.RemoveRange(2, 2); // Simplex = {C,D}

                else
                {
                    // Check what side of ABC D is on to determine what
                    // side of the triangles are outside the tetrahedron
                    float outside = vec3.Dot(vec3.Cross(BC, BA), BD) < 0 ? -1 : 1;

                    if (vec3.Dot(vec3.Cross(B, A), C) * outside < 0 && DotABC_AB > 0 && DotABC_AC > 0 && DotABC_BC > 0) // Origin lies in region ABC
                        tetra.RemoveAt(0); // Simplex = {A,B,C}

                    else if (vec3.Dot(vec3.Cross(D, A), B) * outside < 0 && DotABD_AB > 0 && DotABD_AD > 0 && DotABD_BD > 0) // Origin lies in region ABD
                        tetra.RemoveAt(1); // Simplex = {A,B,D}

                    else if (vec3.Dot(vec3.Cross(C, A), D) * outside < 0 && DotACD_AC > 0 && DotACD_AD > 0 && DotACD_CD > 0) // Origin lies in region ACD
                        tetra.RemoveAt(2); // Simplex = {A,C,D}

                    //else if (vec3.Dot(vec3.Cross(C, D), B) * outside < 0 && DotBCD_BC > 0 && DotBCD_BD > 0 && DotBCD_CD > 0) // Origin lies in region BCD
                    //    tetra.RemoveAt(3); // Simplex = {B,C,D}

                    else return true; // Origin lies within the tetrahedron
                }

            }

            return false;
        }

    }
}
