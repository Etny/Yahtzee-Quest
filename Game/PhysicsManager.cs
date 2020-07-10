using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using GlmSharp;

namespace Yahtzee.Game
{
    class PhysicsManager
    {

        public PhysicsManager()
        {
            /*List<vec3> test = new List<vec3>() { new vec3(0, 2, 1), new vec3(0, 0, 4), new vec3(-2, -1, 1), new vec3(2, -1, 1) };
            vec3 d = new vec3(1);
            bool testBool = SimplexTetraHedron(test, ref d);
            Console.WriteLine($"TH Test: {testBool}");*/
;        }

        public bool GJK(ModelEntity m1, ModelEntity m2)
        {
            List<vec3> simplex = new List<vec3>();
            simplex.Add(SumSupport(m1, m2, vec3.RandomSigned(new Random())));
            vec3 Direction = -simplex[0];

            for(int i = 0; i < 1000; i++)
            {
                vec3 P = SumSupport(m1, m2, Direction);
                if (vec3.Dot(P.Normalized, Direction.Normalized) < 0) return false;
                simplex.Add(P);
                if (DoSimplex(simplex, ref Direction)) return true;
            }

            Console.WriteLine("GJK exceeded max steps!");

            return false;
        }

        private vec3 SumSupport(ModelEntity m1, ModelEntity m2, vec3 Dir)
            => SingleSupport(m1, Dir) - SingleSupport(m2, -Dir);

        private vec3 SingleSupport(ModelEntity m1, vec3 Dir)
        {
            vec3 p = (m1.Transform.ModelMatrix * new vec4(m1.collision.CollisionVertices[0], 1)).xyz;
            float maxDot = -2;

            foreach(vec3 v in m1.collision.CollisionVertices)
            {
                vec3 vm = (m1.Transform.ModelMatrix * new vec4(v, 1)).xyz;
                float dot = vec3.Dot(Dir.Normalized, vm.Normalized);
                if (dot <= maxDot) continue;
                maxDot = dot;
                p = vm;
            }

            return p;
        }

        private bool CheckDir(vec3 v, vec3 referencePoint)
            => vec3.Dot(v.Normalized, (-referencePoint).Normalized) > 0;

        private bool DoSimplex(List<vec3> simplex, ref vec3 Dir)
        {
            switch (simplex.Count)
            {
                case 1:
                    Dir = -simplex[0];
                    return false;

                case 2:
                    return SimplexLine(simplex, ref Dir);

                case 3:
                    return SimplexTriangle(simplex, ref Dir);

                case 4:
                    return SimplexTetraHedron(simplex, ref Dir);
            }

            return false;
        }

        private bool SimplexLine(List<vec3> line, ref vec3 Dir)
        {
            vec3 A = line[1];
            vec3 B = line[0];
            vec3 AB = B - A;

            if (CheckDir(AB, A))
                Dir = vec3.Cross(vec3.Cross(AB, -A), AB);
            else
            {
                Dir = -A;
                line.RemoveAt(0);
            }

            return false;
        }

        private bool SimplexTriangle(List<vec3> tri, ref vec3 Dir)
        {
            vec3 A = tri[2];
            vec3 B = tri[1];
            vec3 C = tri[0];
            vec3 AB = B - A;
            vec3 AC = C - A;
            vec3 Normal = vec3.Cross(AB, AC);

            if (CheckDir(vec3.Cross(Normal, AC), A))
            {
                if (CheckDir(AC, A))
                {
                    tri.RemoveAt(1);
                    Dir = vec3.Cross(vec3.Cross(AC, -A), AC);
                    return false;
                }
                else goto SearchB;
            }
            else if (CheckDir(vec3.Cross(AB, Normal), A))
                goto SearchB;
            else
            {
                if(CheckDir(Normal, A))
                {
                    Dir = Normal;
                    return false;
                }
                else
                {
                    tri[1] = C;
                    tri[0] = B;
                    Dir = -Normal;
                    return false;
                }
            }

            SearchB:
                if (CheckDir(AB, -A))
                {
                    tri.RemoveAt(0);
                    Dir = vec3.Cross(vec3.Cross(AB, -A), AB);
                }
                else
                {
                    tri.RemoveAt(0);
                    tri.RemoveAt(0);
                    Dir = -A;
                }
                
            return false;
        }

        private bool SimplexTetraHedron(List<vec3> tetra, ref vec3 Dir)
        {
            vec3 A = tetra[3];
            vec3 B = tetra[2];
            vec3 C = tetra[1];
            vec3 D = tetra[0];

            vec3 AB = B - A;
            vec3 AC = C - A;
            vec3 AD = D - A;
            vec3 BC = C - B;
            vec3 BD = D - B;
            vec3 CD = D - C;

            //Inward facing normals
            vec3 NormalABC = vec3.Cross(AC, AB);
            vec3 NormalABD = vec3.Cross(AB, AD);
            vec3 NormalACD = vec3.Cross(AD, AC);
            vec3 NormalBCD = vec3.Cross(BC, BD);
            vec3[] normals = { NormalABC, NormalABD, NormalACD, NormalBCD };

            if (CheckDir(NormalABC, A) && CheckDir(NormalABD, A) && CheckDir(NormalACD, A) && CheckDir(NormalBCD, B))
                return true;

            float maxAngle = -1;
            int removeIndex = 0;
            List<vec3> tri = new List<vec3>();

            for (int i = 0; i < 4; i++)
            {
                tri.AddRange(tetra);
                tri.RemoveAt(i);

                float angle = AverageAngleToOrigin(tri, normals[i]);
                if(angle > maxAngle)
                {
                    maxAngle = angle;
                    removeIndex = i;
                }

                tri.Clear();
            }

            tetra.RemoveAt(removeIndex);
            return SimplexTriangle(tetra, ref Dir);
        }

        private float AverageAngleToOrigin(List<vec3> tri, vec3 normal)
        {
            float avg = 0;

            foreach (vec3 p in tri)
                avg += vec3.Dot(normal.Normalized, (-p).Normalized);

            return avg / 3;
        }
    }
}
