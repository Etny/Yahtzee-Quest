using GlmSharp;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using Yahtzee.Game.Debug;
using Yahtzee.Game.Physics.Constraints;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Physics
{
    class RigidBody
    {

        public Entity Parent;
        public CollisionMesh Collision;
        public BoundingBox AABB;
        private static readonly float AABBSkinSize = .1f;
        //public CollisionMesh aabbMesh;

        public bool Sleeping = false;
        public List<int> AABBOverlapCache = new List<int>();
        private vec3 posDelta = vec3.Zero;
        private vec3 rotDelta = vec3.Zero;
        private float timeDelta = 0;
        private float sleepImmunity = SleepImmunityTime;

        private static readonly float PosDeltaSleepThreshold = .2f;
        private static readonly float TimeDeltaSleepThreshold = 2f;
        private static readonly float SleepImmunityTime = 3f;

        public vec3 Position { get { return Parent.Position; } }
        public Transform Transform { get { return Parent.Transform; } }
        public List<int> Overlapping = new List<int>();
        public List<int> OverlappingAABB = new List<int>();
        public List<ConstraintCollision> InvolvedContacts = new List<ConstraintCollision>();

        public vec3 Velocity { get { return _velocity; } set { _velocity = value; _projectedTransformValid = false; } }
        public vec3 AngularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; _projectedTransformValid = false; } }

        private vec3 _velocity = vec3.Zero;
        private vec3 _angularVelocity = vec3.Zero;

        public float Mass = 1;
        public float InverseMass { get { return 1f / Mass; } }

        public mat3 Inertia = mat3.Identity;
        public mat3 InverseInertia = mat3.Identity;
        public mat3 InverseInertiaWorldspace = mat3.Identity;

        public float Friction = 1f;
        public float Restitution = 0f;

        public vec3 ForcesExternal = vec3.Zero;
        public vec3 TorqueExternal = vec3.Zero;

        public bool Static = false;
        public int? Index = null;
        public int UID = -1;

        private Transform _tempTransform;
        private Transform _projectedTransform;
        private bool _projectedTransformValid = false;

        public bool PhysicsActive { get { return !Sleeping && !Static; } }

        public RigidBody(Entity parent, string collision)
        {
            Parent = parent;
            Collision = Model.LoadCollisionMesh(collision);
            AABB = new BoundingBox(vec3.Zero, vec3.Zero);
            //aabbMesh = Model.LoadCollisionMesh(collision);
            UpdateAABB();


            float tr = 1f/6f;
            float of = 0;
            Inertia = new mat3(tr, of, of, of, tr, of, of, of, tr);
            InverseInertia = Inertia.Inverse;
            InverseInertiaWorldspace = InverseInertia;
        }

        ~RigidBody() => Program.PhysicsManager.DeregisterRigidBody(this);

        public void Update(Time deltaTime)
        {
            Collision.Overlapping = false;
            _projectedTransformValid = false;
            Overlapping.Clear();
            OverlappingAABB.Clear();
            InvolvedContacts = InvolvedContacts.FindAll(c => c.StillValid());

            if (Static || Sleeping) return;

            //Apply Gravity
            Impulse(new vec3(0, -4.81f * Mass, 0) * deltaTime.DeltaF, false);
        }

        public void ApplyInitialForces(Time deltaTime)
        {
            if (Static || Sleeping) return;

            _tempTransform = Parent.Transform;
            var dt = deltaTime.DeltaF;

            Velocity += ForcesExternal;
            Parent.Transform.Translation += dt * Velocity;

            AngularVelocity += TorqueExternal;
            Parent.Transform.Orientation = 
                (quat.FromAxisAngle((dt * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * Parent.Transform.Orientation).NormalizedSafe;

            ForcesExternal = vec3.Zero;
            TorqueExternal = vec3.Zero;


            UpdateInertia();
        }

        public void ApplyFinalForces(Time deltaTime)
        {
            if (Sleeping) return;
            if (Static) { UpdateAABB(); return; }

            Parent.Transform = _tempTransform;

            var dt = deltaTime.DeltaF;

            Parent.Transform.Translation += dt * Velocity;
            Parent.Transform.Orientation = 
                (quat.FromAxisAngle((dt * AngularVelocity).Length, AngularVelocity.NormalizedSafe) * Parent.Transform.Orientation).NormalizedSafe;

            UpdateInertia();
            UpdateAABB();
            CheckSleep(deltaTime);
        }

        private void UpdateInertia()
        {
            var t = Parent.Transform;
            t.Translation = vec3.Zero;
            var worldspaceMat = new mat3() { Row0 = t * vec3.UnitX, Row1 = t * vec3.UnitY, Row2 = t * vec3.UnitZ };
            InverseInertiaWorldspace = worldspaceMat.Transposed * InverseInertia * worldspaceMat;
        }

        public void WakeUp()
        {
            if (!Sleeping) return;
            Sleeping = false;
            AABBOverlapCache.Clear();
            sleepImmunity = SleepImmunityTime;
            Collision.NormalColor = new vec3(1, 0.9f, 0);
        }

        private void CheckSleep(Time deltaTime)
        {
            if(sleepImmunity > 0) { sleepImmunity -= deltaTime.DeltaF; return; }

            posDelta += _tempTransform.Translation - Transform.Translation;
            rotDelta += (vec3)(Transform.Orientation * _tempTransform.Orientation.Inverse).EulerAngles;
            
            if((posDelta.Length + rotDelta.Length) >= PosDeltaSleepThreshold)
            {
                posDelta = vec3.Zero;
                rotDelta = vec3.Zero;
                timeDelta = 0;
                return;
            }

            timeDelta += deltaTime.DeltaF;

            if(timeDelta > TimeDeltaSleepThreshold)
            {
                Sleeping = true;
                AABBOverlapCache.AddRange(OverlappingAABB);
                Collision.NormalColor = new vec3(.6f, .1f, .7f);
            }
        }

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

        private void UpdateAABB()
        {
            vec3[] dirs = { vec3.UnitX, vec3.UnitY, vec3.UnitZ,
                           -vec3.UnitX, -vec3.UnitY, -vec3.UnitZ};

            float[] dots = new float[6];
            vec3[] verts = new vec3[6];

            for (int j = 0; j < Collision.CollisionVertices.Length; j++)
            {
                var v = Transform * Collision.CollisionVertices[j];

                for(int i = 0; i < 6; i++)
                {
                    var dir = dirs[i];
                    float d = vec3.Dot(v, dir);
                    if (j != 0 && dots[i] >= d) continue;
                    dots[i] = d;
                    verts[i] = v + (AABBSkinSize * dir);
                }
            }

            AABB.Update(new vec3(verts[0].x, verts[1].y, verts[2].z), new vec3(verts[3].x, verts[4].y, verts[5].z));
        }

        public void Impulse(vec3 impulse, bool wakeup = true)
            => Impulse(impulse, vec3.Zero, wakeup);

        public void ImpulseLocal(vec3 impulse, vec3 point, bool wakeup = true)
            => Impulse(impulse, point + Transform.Translation, wakeup);

        public void Impulse(vec3 impulse, vec3 point, bool wakeup = true)
        {
            if (Static) return;
            if (wakeup) WakeUp();

            ForcesExternal += impulse / Mass;
            if(point != vec3.Zero) TorqueExternal += InverseInertiaWorldspace * vec3.Cross(point, impulse);
        }

    }
}
