using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Core.Debug;
using Yahtzee.Core.Physics;
using Yahtzee.Main;
using db = System.Diagnostics.Debug;

namespace Yahtzee.Core.Physics.Constraints
{
    class ConstraintCollision : IConstraint
    {
        public RigidBody Body1, Body2;
        protected vec3[,] _jacobian;

        public int Age = 0;

        public float TotalLambda { get { return _totalLambda; } }
        public (vec3, vec3) ContactPoints { get { return _contact; } }

        protected vec3 _normal = vec3.NaN;
        protected (vec3, vec3) _contact = (vec3.NaN, vec3.NaN);
        public float _pendepth = 0;
        protected float _oldPendepth = 0;

        protected float _totalLambda = 0;
        protected float _effectiveMass = 0;
        public (LineMesh, LineMesh, int, int) meshes;

        public ConstraintCollision(CollisionResult result)
        {
            Body1 = result.Body1;
            Body2 = result.Body2;

            CalculateData(result);

            Body1.InvolvedContacts.Add(this);
            Body2.InvolvedContacts.Add(this);
        }

        public ConstraintFriction[] GetFrictionConstraints()
        {
            vec3 t, bt;

            if (Math.Abs(_normal.x) >= 1f / Math.Sqrt(3))
                t = new vec3(_normal.y, -_normal.x, 0).NormalizedSafe;
            else
                t = new vec3(0, _normal.z, -_normal.y).NormalizedSafe;

            bt = vec3.Cross(t, _normal);

            return new ConstraintFriction[] { new ConstraintFriction(this, t), new ConstraintFriction(this, bt) };
        }

        public bool StillValid() => Age <= 4 && (Body1.PhysicsActive || Body2.PhysicsActive);

        public void Resolve(Time deltaTime, int iter)
        {
            if (iter == 0)
            { Age++; CalculateJacobian(); }

            if (_pendepth <= 0) return;
            if (!Body1.Overlapping.Contains(Body2.UID)) return;

            //meshes.Item1.SetPoints(new vec3[] { _contact.Item1 });
            //meshes.Item2.SetPoints(new vec3[] { _contact.Item2 });

            float JV = vec3.Dot(_jacobian[0, 0], Body1.Velocity) +
                        vec3.Dot(_jacobian[0, 1], Body1.AngularVelocity) +
                        vec3.Dot(_jacobian[1, 0], Body2.Velocity) +
                        vec3.Dot(_jacobian[1, 1], Body2.AngularVelocity);

            float restitution = Body1.Restitution * Body2.Restitution;
            float closingVelocity = 0;

            if (restitution != 0)
                closingVelocity = vec3.Dot(-Body1.Velocity
                                            - vec3.Cross(Body1.AngularVelocity, _contact.Item1 - Body1.Position)
                                            + Body2.Velocity
                                            + vec3.Cross(Body2.AngularVelocity, _contact.Item2 - Body2.Position),
                                            _normal);


            float bias = .6f / deltaTime.DeltaF * -_pendepth + restitution * closingVelocity;

            float lambda = _effectiveMass * -(JV + bias);

            float oldLambda = _totalLambda;
            _totalLambda = Math.Max(0f, _totalLambda + lambda);
            lambda = _totalLambda - oldLambda;

            if (Body1.PhysicsActive)
                ApplyForces(Body1, deltaTime, Body1.InverseMass * _jacobian[0, 0] * lambda, Body1.InverseInertiaWorldspace * _jacobian[0, 1] * lambda);

            if (Body2.PhysicsActive)
                ApplyForces(Body2, deltaTime, Body2.InverseMass * _jacobian[1, 0] * lambda, Body2.InverseInertiaWorldspace * _jacobian[1, 1] * lambda);
        }

        private void ApplyForces(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot)
        {
            M.Velocity += deltaVel;
            M.AngularVelocity += deltaRot;

            db.Assert(M.InvolvedContacts.Count > 0);

            foreach (ConstraintCollision c in M.InvolvedContacts)
                c.UpdateConstraint(M, deltaTime);
        }


        public void UpdateConstraint(RigidBody M, Time deltaTime)
        {
            if (M != Body1 && M != Body2) return;
            vec3 contact = M == Body1 ? _contact.Item1 : _contact.Item2;

            var newPos = M.ProjectedTransform(deltaTime);

            var r = contact - M.Position;
            var rotated = newPos.Orientation * M.Transform.Orientation.Inverse * r;
            var rot = rotated - r;

            var deltaPos = newPos.Translation - M.Position + rot;


            _pendepth = _oldPendepth + (M == Body1 ? 1 : -1) * vec3.Dot(deltaPos, _normal);
            //if (M == Body1) _contact.Item1 += deltaPos;
            //if (M == Body2) _contact.Item2 += deltaPos;
        }

        public void EndTimestep() => _oldPendepth = _pendepth;

        protected void CalculateJacobian()
        {
            if (_jacobian == null) _jacobian = new vec3[2, 2];

            vec3 r1 = _contact.Item1 - Body1.Position,
                 r2 = _contact.Item2 - Body2.Position;

            _jacobian[0, 0] = Body1.PhysicsActive ? -_normal : vec3.Zero;
            _jacobian[0, 1] = Body1.PhysicsActive ? -vec3.Cross(r1, _normal) : vec3.Zero;
            _jacobian[1, 0] = Body2.PhysicsActive ? _normal : vec3.Zero;
            _jacobian[1, 1] = Body2.PhysicsActive ? vec3.Cross(r2, _normal) : vec3.Zero;


            _effectiveMass = Body1.InverseMass + vec3.Dot(_jacobian[0, 1], Body1.InverseInertiaWorldspace * _jacobian[0, 1]) +
                             Body2.InverseMass + vec3.Dot(_jacobian[1, 1], Body2.InverseInertiaWorldspace * _jacobian[1, 1]);
            _effectiveMass = 1f / _effectiveMass;
        }

        private void CalculateData(CollisionResult result)
        {
            var info = Program.PhysicsManager.DepthDetector.GetPenetrationInfo(result);

            _normal = info.Item1.Normal;
            _contact = Program.PhysicsManager.DepthDetector.GetContactInfo(info);
            _pendepth = info.Item1.ClosestPoint().Length;
            _oldPendepth = _pendepth;

            //meshes = Program.Scene.ContactPointVisualizer.AddPoints(_contact);
        }

    }
}
