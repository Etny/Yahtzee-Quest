using GlmSharp;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using Yahtzee.Game.Physics.Constraints;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Physics
{
    class RigidBody
    {

        public Entity Parent;
        public CollisionMesh Collision;

        public float Friction = 1f;

        public vec3 Position { get { return Parent.Position; } }
        public Transform Transform { get { return Parent.Transform; } }
        public List<int> Overlapping = new List<int>();
        public List<ConstraintCollision> InvolvedContacts = new List<ConstraintCollision>();

        public vec3 Velocity { get { return _velocity; } set { _velocity = value; _projectedTransformValid = false; } }
        public vec3 AngularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; _projectedTransformValid = false; } }

        private vec3 _velocity = vec3.Zero, _angularVelocity = vec3.Zero;

        public float Mass = 1;
        public float InverseMass { get { return 1f / Mass; } }

        public mat3 Inertia = mat3.Identity;
        public mat3 InverseInertia = mat3.Identity;
        public mat3 InverseInertiaWorldspace = mat3.Identity;

        public vec3 ForcesExternal = vec3.Zero;
        public vec3 TorqueExternal = vec3.Zero;

        public bool Static = false;
        public int? Index = null;
        public int UID = -1;

        private Transform _tempTransform;
        private Transform _projectedTransform;
        private bool _projectedTransformValid = false;

        public Transform ProjectedTransform(Time deltaTime)
        {
            if (!_projectedTransformValid)
            {
                _projectedTransform = _tempTransform;
                _projectedTransform.Translation += Velocity * deltaTime.DeltaF;
                _projectedTransform.Orientation = quat.FromAxisAngle((deltaTime.DeltaF * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * _projectedTransform.Orientation;
                _projectedTransformValid = true;
            }

            return _projectedTransform;
        }

        public RigidBody(Entity parent, string collision)
        {
            Parent = parent;
            Collision = Model.LoadCollisionMesh(collision);

            float tr = 1f/6f;
            float of = 0;
            Inertia = new mat3(tr, of, of, of, tr, of, of, of, tr);
            InverseInertia = Inertia.Inverse;
            InverseInertiaWorldspace = InverseInertia;

            //Impulse(new vec3(0, -3000f * Mass, 0), new vec3(-.5f, .5f, 0));
        }

        ~RigidBody() => Program.PhysicsManager.DeregisterRigidBody(this);

        public void Update(Time deltaTime)
        {
            Collision.Overlapping = false;
            _projectedTransformValid = false;
            Overlapping.Clear();
            InvolvedContacts = InvolvedContacts.FindAll(c => c.StillValid());

            if (Static) return;

            //Apply Gravity
            Impulse(new vec3(0, -4.81f * Mass, 0) * deltaTime.DeltaF);
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            if (Static) return;

            _tempTransform = Parent.Transform;
            var dt = deltaTime.DeltaF;

            Velocity += ForcesExternal;
            Parent.Transform.Translation += dt * Velocity;

            AngularVelocity += TorqueExternal;
            Parent.Transform.Orientation = quat.FromAxisAngle((dt * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * Parent.Transform.Orientation;

            ForcesExternal = vec3.Zero;
            TorqueExternal = vec3.Zero;

            UpdateInertia();
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            if (Static) return;

            Parent.Transform = _tempTransform;

            var dt = deltaTime.DeltaF;

            Parent.Transform.Translation += dt * Velocity;
            Parent.Transform.Orientation = quat.FromAxisAngle((dt * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * Parent.Transform.Orientation;

            UpdateInertia();
        }

        private void UpdateInertia()
        {
            var t = Parent.Transform;
            t.Translation = vec3.Zero;
            var worldspaceMat = new mat3() { Row0 = t * vec3.UnitX, Row1 = t * vec3.UnitY, Row2 = t * vec3.UnitZ };
            InverseInertiaWorldspace = worldspaceMat.Transposed * InverseInertia * worldspaceMat;
        }

        public void Impulse(vec3 impulse)
            => Impulse(impulse, vec3.Zero);

        public void ImpulseLocal(vec3 impulse, vec3 point)
            => Impulse(impulse, point + Transform.Translation);

        public void Impulse(vec3 impulse, vec3 point)
        {
            if (Static) return;

            ForcesExternal += impulse / Mass;
            if(point != vec3.Zero) TorqueExternal += InverseInertiaWorldspace * vec3.Cross(point, impulse);
        }

    }
}
