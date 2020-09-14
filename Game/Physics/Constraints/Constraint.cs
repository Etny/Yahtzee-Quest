using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Main;

namespace Yahtzee.Game.Physics.Constraints
{
    abstract class Constraint
    {

        public readonly RigidBody M1, M2;
        public vec3[,] Jacobian { get { if (_jacobian == null) CalculateJacobian(); return _jacobian; } }
        public int Age = 0;

        protected vec3[,] _jacobian = null;
        protected float _bias = 0;
        protected float _biasMultiplier = 0;

        protected Constraint(RigidBody m1, RigidBody m2)
        {
            M1 = m1;
            M2 = m2;
        }

        protected abstract void CalculateJacobian();
        public abstract bool StillValid();

        public abstract void Resolve(Time deltaTime);

        public abstract void UpdateConstraint(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot);

        public float GetEta(Time deltaTime)
        {
            if (_jacobian == null) CalculateJacobian();

            //TODO: actually implement mass
            mat3[] M = { M1.Mass * mat3.Identity, M1.Inertia, M2.Mass * mat3.Identity, M2.Inertia };

            var d = 1 / deltaTime.DeltaF;
            var df = deltaTime.DeltaF;
            vec3[,] V = new vec3[,] { { (d * M1.Velocity) + (df * M1.ForcesExternal), (d * M1.AngularVelocity) + (df * M1.TorqueExternal)},
                                      { (d * M2.Velocity) + (df * M2.ForcesExternal), (d * M2.AngularVelocity) + (df * M2.TorqueExternal)}};

            float result = d * (_biasMultiplier != 0 ? (_biasMultiplier / df) * _bias : _bias);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    result -= vec3.Dot(_jacobian[i, j], V[i, j]);

            return result;
        }
    }
}
