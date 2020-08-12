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
        public float Mass = 1;

        public vec3 ForcesExternal = vec3.Zero;
        public vec3 ForcesInternal = vec3.Zero;

        public int? Index = null;
        public bool Static = false;

        private vec3 _tempPos;


        public RigidBody(Entity parent, string collision)
        {
            Parent = parent;
            Collision = Model.LoadCollisionMesh(collision, parent);
        }

        ~RigidBody() => Program.PhysicsManager.DeregisterRigidBody(this);

        public void Update(Time deltaTime)
        {
            Collision.Overlapping = false;

            //Apply Gravity
            //ForcesExternal += new vec3(0, -(float)(deltaTime.Delta * (9.81f * Mass)), 0);
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            _tempPos = Parent.Transform.Translation;

            Parent.Transform.Translation += (float)deltaTime.Delta * (Velocity + ForcesExternal);
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            Velocity += ForcesExternal + ForcesInternal;
            Parent.Transform.Translation = _tempPos + ((float)deltaTime.Delta * Velocity);

            ForcesInternal = vec3.Zero;
            ForcesInternal = vec3.Zero;
        }

        public void Impulse(vec3 impulse)
            => ForcesExternal += impulse;

    }
}
