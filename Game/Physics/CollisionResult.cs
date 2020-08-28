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

        public vec3 CollisionNormal { get{ if (!Colliding) return vec3.NaN; if (_normal == vec3.NaN) CalculateData(); return _normal; } }
        public vec3 PenetrationVector { get { if (!Colliding) return vec3.NaN; if (_penvec == vec3.NaN) CalculateData(); return _penvec; } }
        public (vec3, vec3) ContactPoints { get { if (!Colliding) return (vec3.NaN, vec3.NaN); if (_contact == (vec3.NaN, vec3.NaN)) CalculateData(); return _contact; } }

        public vec3[,] Jacobian { get { if (!Colliding) return null; if (_jacobian == null) CalculateJacobian(); return _jacobian; } }


        private vec3 _normal = vec3.NaN;
        private vec3 _penvec = vec3.NaN;
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

            //_normal = new vec3(0, -1, 0);
            //_normal = (x2 - x1).NormalizedSafe;
            //Console.WriteLine(_normal);
            _error = vec3.Dot(x2 + r2 - x1 - r1, _normal);

            db.Assert(!Colliding || _error <= 0);
            //if (_error > 0) _error = 0;

            //Console.WriteLine(_contact.Item1 + " || " + _contact.Item2 + " || " + (_contact.Item1 + _penvec));
            //Console.WriteLine(_error);

            _jacobian[0, 0] = -_normal;
            _jacobian[0, 1] = -vec3.Cross(r1, _normal);
            _jacobian[1, 0] = _normal;
            _jacobian[1, 1] = vec3.Cross(r2, _normal);


            db.Assert(_jacobian[0, 0].Length > 0);
        }

        public float GetEta(Time deltaTime)
        {
            if (!Colliding) return 0;
            if (_jacobian == null) CalculateJacobian();

            //TODO: actually implement mass
            mat3[] M = { M1.Mass * mat3.Identity, M1.Inertia, M2.Mass * mat3.Identity, M2.Inertia };
            //foreach (mat3 m in M) foreach (vec3 v in new vec3[] { m.Row0, m.Row1, m.Row2 }) Console.WriteLine(v);
            //Console.WriteLine("--------------");

            var d = 1 / deltaTime.DeltaF;
            var df = 1; // deltaTime.DeltaF;
            vec3[,] V = new vec3[,] { { (d * M1.Velocity) + (df * M1.ForcesExternal), (d * M1.AngularVelocity) + (df * M1.TorqueExternal)},
                                      { (d * M2.Velocity) + (df * M2.ForcesExternal), (d * M2.AngularVelocity) + (df * M2.TorqueExternal)}};

            float result = d * (-(.3f / deltaTime.DeltaF) * _error);
            //result = d * _penvec.Length;

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    result -= vec3.Dot(_jacobian[i, j], V[i, j]);
                
            return result;
        }

        private void CalculateData()
        {
            if (!Colliding) return;

            var info = Program.PhysicsManager.DepthDetector.NewEPA(this);

            _normal = info.Item1.Normal;
            //_penvec = info.Item2;
            _contact = Program.PhysicsManager.DepthDetector.ContactDouble(info);

            db.Assert(_normal != vec3.NaN);
        }
    }
}
