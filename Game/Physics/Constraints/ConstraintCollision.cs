using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Game.Debug;
using Yahtzee.Main;
using db = System.Diagnostics.Debug;

namespace Yahtzee.Game.Physics.Constraints
{
    class ConstraintCollision : Constraint
    {
        protected vec3 _normal = vec3.NaN;
        protected (vec3, vec3) _contact = (vec3.NaN, vec3.NaN);
        protected float _pendepth = 0;

        private float tl = 0;
        private float k = 0;
        public (LineMesh, LineMesh) meshes;

        public ConstraintCollision(CollisionResult result) : base(result.M1, result.M2) 
        {
            M1.InvolvedConstraints.Add(this);
            M2.InvolvedConstraints.Add(this);

            CalculateData(result);
        }

        public ConstraintCollision(CollisionResult result, vec3 c1, vec3 c2) : this(result)
        {
            _contact.Item1 = c1;
            _contact.Item2 = c2;

            vec3 r1 = _contact.Item1 - M1.Transform.Translation,
                 r2 = _contact.Item2 - M2.Transform.Translation;

            _pendepth = -vec3.Dot(M2.Transform.Translation + r2 - M1.Transform.Translation - r1, _normal);
        }

        public override bool StillValid()
        {
            if (!M1.Overlapping.Contains(M2.UID)) return false;
            if (Age >= 6) return false;

            return true;
        }

        public override void Resolve(Time deltaTime)
        {
            if (_pendepth <= 0) return;

            //meshes.Item1.SetPoints(new vec3[] { _contact.Item1 });
            //meshes.Item2.SetPoints(new vec3[] { _contact.Item2 });

            CalculateJacobian(deltaTime);

            float JV =  vec3.Dot(_jacobian[0, 0], M1.Velocity) +
                        vec3.Dot(_jacobian[0, 1], M1.AngularVelocity) +
                        vec3.Dot(_jacobian[1, 0], M2.Velocity) +
                        vec3.Dot(_jacobian[1, 1], M2.AngularVelocity);

            float bias = (.5f / deltaTime.DeltaF) * -_pendepth;

            float lambda = k * (-(JV + bias));

            float oldLambda = tl;
            tl = Math.Max(0f, tl + lambda);
            lambda = tl - oldLambda;

            ApplyForces(M1, deltaTime, _jacobian[0, 0] * lambda, _jacobian[0, 1] * lambda);

            if (!M2.Static)
                ApplyForces(M2, deltaTime, _jacobian[1, 0] * lambda, _jacobian[1, 1] * lambda);
        }

        private void ApplyForces(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot)
        {
            M.ForcesConstraints += deltaVel;
            M.TorqueConstraints += deltaRot;

            foreach (Constraint c in M.InvolvedConstraints)
                    c.UpdateConstraint(M, deltaTime, deltaVel, deltaRot);
        }

        public override void UpdateConstraint(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot)
        {
            if (M != M1 && M != M2) return;
            vec3 contact = M == M1 ? _contact.Item1 : _contact.Item2;

            var rot = (quat.FromAxisAngle((deltaTime.DeltaF * deltaRot).Length, deltaRot.NormalizedSafe) * contact) - contact;
            var deltaPos = (deltaVel * deltaTime.DeltaF) + rot;

            //Console.Write("Old pendepth :" + _pendepth);
            _pendepth += (M == M1 ? 1 : -1) * vec3.Dot(deltaPos, _normal);
            //Console.WriteLine(", New pendepth: " + _pendepth + ", deltaPos: "+deltaPos + ", actual: "+(vec3.Dot(deltaPos, _normal)));
        }



        protected void CalculateJacobian(Time deltaTime)
        {
            if(_jacobian == null) _jacobian = new vec3[2, 2];

            vec3 r1 = _contact.Item1 - (M1.Transform.Translation + (deltaTime.DeltaF * M1.ForcesConstraints)), 
                 r2 = _contact.Item2 - (M2.Transform.Translation + (deltaTime.DeltaF * M2.ForcesConstraints));

            _jacobian[0, 0] = -_normal;
            _jacobian[0, 1] = -vec3.Cross(r1, _normal);
            _jacobian[1, 0] = _normal;
            _jacobian[1, 1] = vec3.Cross(r2, _normal);

            
            k = M1.InverseMass + vec3.Dot(_jacobian[0, 1], M1.InverseInertia * _jacobian[0, 1]) +
                M2.InverseMass + vec3.Dot(_jacobian[1, 1], M2.InverseInertia * _jacobian[1, 1]);
            k = 1f / k;
        }

        private void CalculateData(CollisionResult result)
        {
            var info = Program.PhysicsManager.DepthDetector.GetPenetrationInfo(result);

            _normal = info.Item1.Normal;
            _contact = Program.PhysicsManager.DepthDetector.GetContactInfo(info);
            _pendepth = info.Item1.DistToOrigin();

            //meshes = Program.Scene.ContactPointVisualizer.AddPoints(_contact);
        }

    }
}
