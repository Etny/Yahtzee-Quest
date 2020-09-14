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
        public (LineMesh, LineMesh) meshes;

        public ConstraintCollision(CollisionResult result) : base(result.M1, result.M2) 
        {
            M1.InvolvedConstraints.Add(this);
            M2.InvolvedConstraints.Add(this);

            CalculateData(result);
            _biasMultiplier = -.3f;
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
            if (Age >= 3) return false;

            return _pendepth > 0;
        }

        public override void Resolve(Time deltaTime)
        {
            if (!StillValid()) return;

            /*if (_jacobian == null)*/ CalculateJacobian();

            float JV =  vec3.Dot(_jacobian[0, 0], M1.Velocity) +
                        vec3.Dot(_jacobian[0, 1], M1.AngularVelocity) +
                        vec3.Dot(_jacobian[1, 0], M2.Velocity) +
                        vec3.Dot(_jacobian[1, 1], M2.AngularVelocity);

            float JJt = _jacobian[0, 0].LengthSqr +
                        _jacobian[0, 1].LengthSqr +
                        _jacobian[1, 0].LengthSqr +
                        _jacobian[1, 1].LengthSqr;

            float bias = (.3f / deltaTime.DeltaF) * -_pendepth;

            float lambda = -(JV + bias) / JJt;

            float oldLambda = tl;
            tl = Math.Max(0f, tl + lambda);
            lambda = tl - oldLambda;

            ApplyForces(M1, deltaTime, _jacobian[0, 0] * lambda, _jacobian[0, 1] * lambda);

            if (!M2.Static)
                ApplyForces(M2, deltaTime, _jacobian[1, 0] * lambda, _jacobian[1, 1] * lambda);
        }

        private void ApplyForces(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot)
        {
            M.Velocity += deltaVel;
            M.AngularVelocity += deltaRot;

            foreach (Constraint c in M.InvolvedConstraints)
                if (!c.Equals(this))
                    c.UpdateConstraint(M, deltaTime, deltaVel, deltaRot);
        }

        public override void UpdateConstraint(RigidBody M, Time deltaTime, vec3 deltaVel, vec3 deltaRot)
        {
            if (!StillValid()) return;
            if (M != M1 && M != M2) return;

            bool firstIndex = M == M1;
            vec3 contact = firstIndex ? _contact.Item1 : _contact.Item2;

            var tq = M.Transform.Rotation;
            var ts = .5f * new quat(deltaRot * deltaTime.DeltaF, 0) * tq * deltaTime.DeltaF;
            var rot = M.Transform.Rotation - (tq + ts).NormalizedSafe;
            //Console.WriteLine(ts + " ||| " + rot);
            var deltaPos = (deltaVel * deltaTime.DeltaF * deltaTime.DeltaF) /*+ vec3.Cross((vec3)(tq + ts).NormalizedSafe.EulerAngles, contact - M.Position)*/;

            //Console.Write("Old pendepth :" + _pendepth);
            _pendepth += (firstIndex ? 1 : -1) * vec3.Dot(deltaPos, _normal);
            //Console.WriteLine(", New pendepth: " + _pendepth + ", deltaPos: "+deltaPos + ", actual: "+(vec3.Dot(deltaPos, _normal)));

            if (firstIndex)
                _contact.Item1 += deltaPos;
            
            else _contact.Item2 += deltaPos;

            if ((_contact.Item1 - _contact.Item2).Normalized != _normal) _contact.Item2 = _contact.Item1 - (_pendepth * _normal);

            meshes.Item1.SetPoints(new vec3[] { _contact.Item1 });
            meshes.Item2.SetPoints(new vec3[] { _contact.Item2 });

        }



        protected override void CalculateJacobian()
        {
            _jacobian = new vec3[2, 2];

            vec3 r1 = _contact.Item1 - M1.Transform.Translation, 
                 r2 = _contact.Item2 - M2.Transform.Translation;

            _jacobian[0, 0] = -_normal;
            _jacobian[0, 1] = -vec3.Cross(r1, _normal);
            _jacobian[1, 0] = !M2.Static ? _normal : -_normal; //This stops broken interaction with static objects. I don't know why, but it does, so whatevs.
            _jacobian[1, 1] = !M2.Static ? vec3.Cross(r2, _normal) : -vec3.Cross(r1, _normal);
        }

        private void CalculateData(CollisionResult result)
        {
            var info = Program.PhysicsManager.DepthDetector.GetPenetrationInfo(result);

            _normal = info.Item1.Normal;
            _contact = Program.PhysicsManager.DepthDetector.GetContactInfo(info);
            _pendepth = info.Item1.ClosestPoint().Length;

            meshes = Program.Scene.ContactPointVisualizer.AddPoints(_contact);

            //Console.WriteLine(_normal + " VS " + (_contact.Item1 - _contact.Item2).Normalized);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConstraintCollision)) return false;
            var other = (ConstraintCollision)obj;

            return other._pendepth == _pendepth && other._normal == _normal && other._contact == _contact && other.M1.UID == M1.UID;
        }
    }
}
