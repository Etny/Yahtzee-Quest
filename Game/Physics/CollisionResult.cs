using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlmSharp;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Yahtzee.Main;
using db = System.Diagnostics.Debug;

namespace Yahtzee.Game.Physics
{
    class CollisionResult
    {
        public readonly RigidBody M1, M2;
        public readonly List<SupportPoint> Simplex;

        public vec3[,] Jacobian { get { if (!Colliding) return null; if (_jacobian == null) CalculateJacobian(); return _jacobian; } }


        private vec3 _normal = vec3.NaN;
        private (vec3, vec3) _contact = (vec3.NaN, vec3.NaN);
        private vec3[,] _jacobian = null;
        private float _error = 0;

        public IEnumerable<vec3> SimplexPos { get { return Simplex.Select(p => p.Sup); } }

        public bool Colliding;

        public CollisionResult(RigidBody m1, RigidBody m2, List<SupportPoint> simplex, bool colliding = false)
        {
            M1 = m1;
            M2 = m2;
            Simplex = simplex;
            Colliding = colliding;
        }

        private void CalculateJacobian()
        {
            if (!Colliding) return;
            if (_normal == vec3.NaN) CalculateData();

            _jacobian = new vec3[2,2];

            vec3 x1 = M1.Transform.Translation, x2 = M2.Transform.Translation;
            vec3 r1 = _contact.Item1 - x1, r2 = _contact.Item2 - x2;

            _error = vec3.Dot(x2 + r2 - x1 - r1, _normal);

            db.Assert(!Colliding || _error <= 0);

            _jacobian[0, 0] = -_normal;
            _jacobian[0, 1] = -vec3.Cross(r1, _normal);
            _jacobian[1, 0] = !M2.Static ? _normal : -_normal;
            _jacobian[1, 1] = !M2.Static ? vec3.Cross(r2, _normal) : -vec3.Cross(r1, _normal);

            //Console.WriteLine("Jac Rot: " + _jacobian[0, 1] + ", " + _jacobian[1, 1]);
        }

        public float GetEta(Time deltaTime)
        {
            if (!Colliding) return 0;
            if (_jacobian == null) CalculateJacobian();

            //TODO: actually implement mass
            mat3[] M = { M1.Mass * mat3.Identity, M1.Inertia, M2.Mass * mat3.Identity, M2.Inertia };

            var d = 1 / deltaTime.DeltaF;
            var df = deltaTime.DeltaF;
            vec3[,] V = new vec3[,] { { (d * M1.Velocity) + (df * M1.ForcesExternal), (d * M1.AngularVelocity) + (df * M1.TorqueExternal)},
                                      { (d * M2.Velocity) + (df * M2.ForcesExternal), (d * M2.AngularVelocity) + (df * M2.TorqueExternal)}};

            float result = d * (-(.3f / deltaTime.DeltaF) * _error);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    result -= vec3.Dot(_jacobian[i, j], V[i, j]);

            //Console.WriteLine(result);

            return result;
        }

        private void CalculateData()
        {
            if (!Colliding) return;
           
            var info = Program.PhysicsManager.DepthDetector.GetPenetrationInfo(this);

            //TODO: Remove this
            if(info.Item1 == null)
            {
                _normal = -vec3.UnitY;
                _contact = (vec3.UnitX, vec3.UnitZ);
                M1.Static = true;
                return;
            }

            _normal = info.Item1.Normal;
            _contact = Program.PhysicsManager.DepthDetector.GetContactInfo(info);


            db.Assert(_normal != vec3.NaN);
        }
    }
}
