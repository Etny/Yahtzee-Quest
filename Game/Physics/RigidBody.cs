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

        public vec3 Position { get { return Parent.Position; } }
        public Transform Transform { get { return Parent.Transform; } }
        public List<int> Overlapping = new List<int>();
        public List<Constraint> InvolvedConstraints = new List<Constraint>();

        public vec3 Velocity = vec3.Zero;
        public vec3 AngularVelocity = vec3.Zero;

        public float Mass = 1;
        public mat3 Inertia = mat3.Identity;
        public mat3 InertiaInverse = mat3.Identity;

        public vec3 ForcesExternal = vec3.Zero;
        public vec3 ForcesConstraints = vec3.Zero;

        public vec3 TorqueExternal = vec3.Zero;
        public vec3 TorqueConstraints = vec3.Zero;

        public bool Static = false;
        public int? Index = null;
        public int UID = -1;

        private Transform _tempTransform;


        public RigidBody(Entity parent, string collision)
        {
            Parent = parent;
            Collision = Model.LoadCollisionMesh(collision, parent);

            InertiaInverse = Inertia.Inverse;

            //Impulse(new vec3(0, -3000f * Mass, 0), new vec3(-.5f, .5f, 0));
        }

        ~RigidBody() => Program.PhysicsManager.DeregisterRigidBody(this);

        public void Update(Time deltaTime)
        {
            Collision.Overlapping = false;
            Overlapping.Clear();
            InvolvedConstraints = InvolvedConstraints.FindAll(c => c.StillValid());

            if (Static) return;

            //Apply Gravity
            Impulse(new vec3(0, -4.81f * Mass, 0));
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            if (Static) return;

            _tempTransform = Parent.Transform;

            Parent.Transform.Translation += deltaTime.DeltaF * (Velocity + ((deltaTime.DeltaF * ForcesExternal) / Mass));
            Parent.Transform.Rotation += .5f * new quat(AngularVelocity + (InertiaInverse * (deltaTime.DeltaF * TorqueExternal)), 0) * Parent.Transform.Rotation * deltaTime.DeltaF;
            Parent.Transform.Rotation = Parent.Transform.Rotation.NormalizedSafe;
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            if (Static) return;

            Parent.Transform = _tempTransform;

            Velocity += (deltaTime.DeltaF * (ForcesExternal + ForcesConstraints)) / Mass;
            Parent.Transform.Translation += deltaTime.DeltaF * Velocity;

            AngularVelocity += InertiaInverse * (deltaTime.DeltaF * (TorqueExternal + TorqueConstraints));
            Parent.Transform.Rotation += .5f * new quat(AngularVelocity, 0) * Parent.Transform.Rotation * deltaTime.DeltaF;
            Parent.Transform.Rotation = Parent.Transform.Rotation.NormalizedSafe;

            ForcesExternal = vec3.Zero;
            ForcesConstraints = vec3.Zero;

            TorqueExternal = vec3.Zero;
            TorqueConstraints = vec3.Zero;
        }

        public void Impulse(vec3 impulse)
            => Impulse(impulse, vec3.Zero);

        public void ImpulseLocal(vec3 impulse, vec3 point)
            => Impulse(impulse, point + Transform.Translation);

        public void Impulse(vec3 impulse, vec3 point)
        {
            if (Static) return;

            ForcesExternal += impulse;
            if(point != vec3.Zero) TorqueExternal += vec3.Cross(point, impulse);
        }

    }
}
