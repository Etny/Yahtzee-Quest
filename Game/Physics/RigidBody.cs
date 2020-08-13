using GlmSharp;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Text;
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

        public vec3 Velocity = vec3.Zero;
        public vec3 AngularVelocity = vec3.Zero;

        public float Mass = 1;
        public mat3 Inertia = mat3.Identity;
        public mat3 InertiaInverse = mat3.Identity;

        public vec3 ForcesExternal = vec3.Zero;
        public vec3 ForcesInternal = vec3.Zero;

        public vec3 TorqueExternal = vec3.Zero;
        public vec3 TorqueInternal = vec3.Zero;

        public int? Index = null;
        public bool Static = false;

        private Transform _tempTransform;
        

        public RigidBody(Entity parent, string collision)
        {
            Parent = parent;
            Collision = Model.LoadCollisionMesh(collision, parent);

            InertiaInverse = Inertia.Inverse;

            //Impulse(new vec3(0, -30f * Mass, 0), new vec3(0, .5f, -.5f));
        }

        ~RigidBody() => Program.PhysicsManager.DeregisterRigidBody(this);

        public void Update(Time deltaTime)
        {
            Collision.Overlapping = false;

            //Apply Gravity
            //Impulse(new vec3(0, -9.81f * Mass, 0));
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            if (Static) return;

            _tempTransform = Parent.Transform;

            Parent.Transform.Translation += deltaTime.DeltaF * (Velocity + (deltaTime.DeltaF*(ForcesExternal/Mass)));
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            if (Static) return;
            
            Parent.Transform = _tempTransform;

            Velocity += deltaTime.DeltaF * ((ForcesExternal + ForcesInternal) / Mass);
            //Parent.Transform.Translation += deltaTime.DeltaF * Velocity;

            AngularVelocity += deltaTime.DeltaF * (InertiaInverse * (TorqueExternal + TorqueInternal));
            Parent.Transform.Rotation += Transform.Rotation * (new quat(AngularVelocity, 0) / 2);

            ForcesExternal = vec3.Zero;
            ForcesInternal = vec3.Zero;

            TorqueExternal = vec3.Zero;
            TorqueInternal = vec3.Zero;
        }
        
        public void Impulse(vec3 impulse)
            => Impulse(impulse, vec3.Zero);

        public void ImpulseWorldSpace(vec3 impulse, vec3 point)
            => Impulse(impulse, point - Transform.Translation);

        public void Impulse(vec3 impulse, vec3 point)
        {
            if (Static) return;

            ForcesExternal += impulse;
            TorqueExternal += vec3.Cross(point, impulse);
        }

    }
}
