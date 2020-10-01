using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Game.Physics.Constraints
{
    class ConstraintFriction : IConstraint
    {
        protected vec3[,] _jacobian;
        protected vec3 _normal;

        protected ConstraintCollision Contact;
        protected RigidBody Body1, Body2;

        protected float _effectiveMass = 0;
        protected float _totalLambda = 0;
        


        public ConstraintFriction(ConstraintCollision contact, vec3 normal)
        {
            Contact = contact;
            Body1 = contact.Body1;
            Body2 = contact.Body2;

            _normal = normal;
        }

        public void Resolve(Time deltaTime, int iter)
        {
            if (iter != 0) return;
            if (Contact._pendepth < -0.2) return;

            CalculateJacobian(deltaTime);

            float JV =  vec3.Dot(_jacobian[0, 0], Body1.Velocity) +
                        vec3.Dot(_jacobian[0, 1], Body1.AngularVelocity) +
                        vec3.Dot(_jacobian[1, 0], Body2.Velocity) +
                        vec3.Dot(_jacobian[1, 1], Body2.AngularVelocity);

            float lambda = _effectiveMass * -JV;

            float totalFriction = (Body1.Friction * Body2.Friction) * Contact.TotalLambda;

            float oldLambda = _totalLambda;
            _totalLambda = Math.Clamp(_totalLambda + lambda, -totalFriction, totalFriction);
            lambda = _totalLambda - oldLambda;

            //Console.WriteLine("Friction: " + Body1.InverseInertiaWorldspace * _jacobian[0, 1] * lambda + " on step " + deltaTime.Step);

            if (Body1.PhysicsActive)
                ApplyForces(Body1, Body1.InverseMass * _jacobian[0, 0] * lambda, Body1.InverseInertiaWorldspace * _jacobian[0, 1] * lambda);

            if (Body2.PhysicsActive)
                ApplyForces(Body2, Body2.InverseMass * _jacobian[1, 0] * lambda, Body2.InverseInertiaWorldspace * _jacobian[1, 1] * lambda);
        }

        private void ApplyForces(RigidBody M, vec3 deltaVel, vec3 deltaRot)
        {
            M.Velocity += deltaVel;
            M.AngularVelocity += deltaRot;
        }

        protected void CalculateJacobian(Time deltaTime)
        {
            if (_jacobian == null) _jacobian = new vec3[2, 2];

            vec3 r1 = Contact.ContactPoints.Item1 - Body1.Position,
                 r2 = Contact.ContactPoints.Item2 - Body2.Position;

            _jacobian[0, 0] = Body1.PhysicsActive ? -_normal : vec3.Zero;
            _jacobian[0, 1] = Body1.PhysicsActive ? -vec3.Cross(r1, _normal) : vec3.Zero;
            _jacobian[1, 0] = Body2.PhysicsActive ? _normal : vec3.Zero;
            _jacobian[1, 1] = Body2.PhysicsActive ? vec3.Cross(r2, _normal) : vec3.Zero;

            _effectiveMass = Body1.InverseMass + vec3.Dot(_jacobian[0, 1], Body1.InverseInertiaWorldspace * _jacobian[0, 1]) +
                             Body2.InverseMass + vec3.Dot(_jacobian[1, 1], Body2.InverseInertiaWorldspace * _jacobian[1, 1]);
            _effectiveMass = 1f / _effectiveMass;
        }

        public bool StillValid() => Contact.StillValid();
    }
}
