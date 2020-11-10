﻿using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Curve;
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

        public ICurve LerpCurve;
        public float LerpDuration = .6f;

        public readonly vec3[] NumberFaces = new vec3[]{vec3.UnitX, vec3.UnitY, vec3.UnitZ,
                                                        -vec3.UnitX, -vec3.UnitY, -vec3.UnitZ};
        public readonly int[] Numbers = new int[] { 6, 2, 3, 5, 4, 1 };

        public vec3 CameraOffset = vec3.Zero;

        private int _rolledIndex = 0;

        public EntityDie(string modelPath) : base(modelPath)
        {
            RigidBodyController = new MovementControllerRigidBody(this, false);
            RigidBody.CollisionTransform.Scale = new vec3(.97f);

            LerpCurve = new BezierCurve(new vec2(1, 0), new vec2(.61f, .94f));

            //dm = new LineMesh(colors: new vec3[] { new vec3(.7f, .2f, .3f), new vec3(.7f, .2f, .3f) });
        }

        public void EnablePhysics()
        {
            RigidBodyController.RigidBody.ResetVelocities();
            RigidBodyController.Register();
            MovementController = RigidBodyController;
        }

        public void DisablePhysics()
        {
            if (UsingRigidBody)
                MovementController = null;

            RigidBodyController.Deregister();
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
            DisablePhysics();

            var camera = Program.CurrentScene.CurrentCamera;
            Transform targetTransform = Transform;

            targetTransform.Translation = camera.Transform * CameraOffset;

            vec3 face = vec3.UnitZ;
            vec3 rolledFace = GetRolledFace();
            vec3 axis = rolledFace == face || -rolledFace == face ? vec3.UnitY : vec3.Cross(face, rolledFace).NormalizedSafe;
            quat faceRot = quat.FromAxisAngle(-(float)Math.Acos(Math.Clamp(vec3.Dot(face, rolledFace), -1, 1)), axis);
            targetTransform.Orientation = faceRot;
            targetTransform.Orientation = (camera.Transform.Orientation * targetTransform.Orientation).NormalizedSafe;

            LerpController = new MovementControllerLerp(Transform, targetTransform, speed: LerpDuration) { Curve = LerpCurve, Lerping = true };
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
