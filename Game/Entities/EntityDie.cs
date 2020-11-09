using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Physics;
using Yahtzee.Game.Physics;
using Yahtzee.Main;
using Yahtzee.Render;
using db = System.Diagnostics.Debug;

namespace Yahtzee.Game.Entities
{
    class EntityDie : ModelEntity
    {

        public bool UsingRigidBody { get { return MovementController is MovementControllerRigidBody; } }
        public RigidBody RigidBody { get { return RigidBodyController.RigidBody; } }

        public MovementControllerLerp LerpController;
        public MovementControllerRigidBody RigidBodyController;

        public readonly vec3[] NumberFaces = new vec3[]{vec3.UnitX, vec3.UnitY, vec3.UnitZ,
                                                        -vec3.UnitX, -vec3.UnitY, -vec3.UnitZ};
        public readonly int[] Numbers = new int[] { 6, 2, 3, 5, 4, 1 };

        public vec3 CameraOffset = vec3.Zero;

        private int _rolledIndex = 0;

        public EntityDie(string modelPath) : base(modelPath)
        {
            RigidBodyController = new MovementControllerRigidBody(this);
            RigidBody.CollisionTransform.Scale = new vec3(.97f);

            //MovementController = RigidBodyController;

            //dm = new LineMesh(colors: new vec3[] { new vec3(.7f, .2f, .3f), new vec3(.7f, .2f, .3f) });
        }

        public void EnablePhysics()
        {
            RigidBodyController.RigidBody.ResetVelocities();
            MovementController = RigidBodyController;
        }

        public void CalculateRolledIndex()
        {
            float dot = 0;
            int index = 0;

            for (int i = 0; i < 6; i++)
            {
                float d = vec3.Dot(Transform.Orientation * NumberFaces[i], vec3.UnitY);
                if (i != 0 && d < dot) continue;
                dot = d;
                index = i;
            }

            _rolledIndex = index;
        }

        public int GetRolledFaceIndex() => _rolledIndex;

        public vec3 GetRolledFace() => NumberFaces[_rolledIndex];
        public int GetRolledNumber() => Numbers[_rolledIndex];

        public void StartLerpToCamera()
        {
            var camera = Program.CurrentScene.CurrentCamera;
            Transform targetTransform = Transform;

            //targetTransform.Scale = new vec3(.35f);

            targetTransform.Translation = camera.Transform * CameraOffset;

            vec3 face = vec3.UnitZ;
            vec3 rolledFace = GetRolledFace();
            vec3 axis = rolledFace == face || -rolledFace == face ? vec3.UnitY : vec3.Cross(face, rolledFace).NormalizedSafe;
            quat faceRot = quat.FromAxisAngle(-(float)Math.Acos(Math.Clamp(vec3.Dot(face, rolledFace), -1, 1)), axis);
            targetTransform.Orientation = faceRot;
            targetTransform.Orientation = (camera.Transform.Orientation * targetTransform.Orientation).NormalizedSafe;

            RigidBody.Deregister();

            LerpController = new MovementControllerLerp(Transform, targetTransform, speed: .6f) { Lerping = true };
            MovementController = LerpController;
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            if ((MovementController as MovementControllerLerp) == null) return;

            //var camera = Program.Scene.CurrentCamera;
            //Transform targetTransform = Transform;

            //targetTransform.Translation = camera.Transform * CameraOffset;

            //vec3 face = vec3.UnitZ;
            //vec3 rolledFace = GetRolledFace();
            //vec3 axis = rolledFace == face || -rolledFace == face ? vec3.UnitY : vec3.Cross(face, rolledFace).NormalizedSafe;
            //quat faceRot = quat.FromAxisAngle(-(float)Math.Acos(Math.Clamp(vec3.Dot(face, rolledFace), -1, 1)), axis);
            //targetTransform.Orientation = faceRot;
            //targetTransform.Orientation = (camera.Transform.Orientation * targetTransform.Orientation).NormalizedSafe;

            //Transform = targetTransform;
        }

        public override void Draw(Shader shader)
        {
            //RigidBody.aabbMesh.DrawOutline(RigidBody.AABB.GetTransform());
            //((MovementControllerRigidBody)MovementController).Collision.DrawOutline(RigidBody.Transform);
            //dm.Draw(null);

            base.Draw(shader);
        }
    }
}
