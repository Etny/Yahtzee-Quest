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
        public int Age = 0;

        protected vec3[,] _jacobian = null;

        protected Constraint(RigidBody m1, RigidBody m2)
        {
            M1 = m1;
            M2 = m2;
        }

        //protected abstract void CalculateJacobian();
        public abstract bool StillValid();

        public abstract void Resolve(Time deltaTime);

        public abstract void UpdateConstraint(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot);

        
    }
}
