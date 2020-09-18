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
        public float InverseMass { get { return 1f / Mass; } }

        public mat3 Inertia = mat3.Identity;
        public mat3 InverseInertia = mat3.Identity;

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

            InverseInertia = Inertia.Inverse;

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
            Impulse(new vec3(0, -4.81f * Mass, 0) * deltaTime.DeltaF);
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            if (Static) return;

            _tempTransform = Parent.Transform;

            Velocity += ForcesExternal;
            Parent.Transform.Translation += deltaTime.DeltaF * Velocity;

            AngularVelocity += TorqueExternal;
            var theta = (deltaTime.DeltaF * AngularVelocity).Length;
            var a = AngularVelocity.NormalizedSafe;
            Parent.Transform.Rotation = new quat(a * (float)Math.Sin(theta / 2), (float)Math.Cos(theta / 2)) * Parent.Transform.Rotation;
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            if (Static) return;

            Parent.Transform = _tempTransform;

            var dt = deltaTime.DeltaF;

            Velocity += ForcesConstraints;
            Parent.Transform.Translation += dt * Velocity;

            AngularVelocity += TorqueConstraints;
            Parent.Transform.Rotation = quat.FromAxisAngle((dt * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * Parent.Transform.Rotation;


            ForcesExternal = vec3.Zero;
            ForcesConstraints = vec3.Zero;

            TorqueExternal = vec3.Zero;
            TorqueConstraints = vec3.Zero;

            //Console.WriteLine(Velocity.x);
        }

        public void Impulse(vec3 impulse)
            => Impulse(impulse, vec3.Zero);

        public void ImpulseLocal(vec3 impulse, vec3 point)
            => Impulse(impulse, point + Transform.Translation);

        public void Impulse(vec3 impulse, vec3 point)
        {
            if (Static) return;

            ForcesExternal += impulse / Mass;
            if(point != vec3.Zero) TorqueExternal += InverseInertia * vec3.Cross(point, impulse);
        }

    }
}
