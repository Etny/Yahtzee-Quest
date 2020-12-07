using GlmSharp;
using Silk.NET.OpenGL;
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

        public Outliner Outliner;

        public ICurve LerpCurve;

        public readonly vec3[] NumberFaces = new vec3[]{vec3.UnitX, vec3.UnitY, vec3.UnitZ,
                                                        -vec3.UnitX, -vec3.UnitY, -vec3.UnitZ};
        public readonly int[] Numbers = new int[] { 6, 2, 3, 5, 4, 1 };

        private int _rolledIndex = 0;

        public EntityDie(GL gl, string modelPath) : base(modelPath)
        {
            RigidBodyController = new MovementControllerRigidBody(this, false);
            RigidBody.CollisionTransform.Scale = new vec3(.97f);

            LerpCurve = new BezierCurve(new vec2(1, 0), new vec2(.61f, .94f));

            Outliner = new Outliner(gl, this)
            {
                Enabled = false
            };

            DrawInstanced = false;
            //dm = new LineMesh(colors: new vec3[] { new vec3(.7f, .2f, .3f), new vec3(.7f, .2f, .3f) });
        }

        public void EnablePhysics()
        {
            RigidBodyController.Register();
            RigidBodyController.RigidBody.Reset();
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

        public void Lerp(Transform t, float duration)
        {
            DisablePhysics();

            LerpController = new MovementControllerLerp(Transform, t, speed: duration) { Curve = LerpCurve, Lerping = true };
            LerpController.OnComplete += RemoveLerp;
            MovementController = LerpController;
        }

        public void LerpToPoint(vec3 point, float duration)
        {
            Transform t = Transform;
            t.Translation = point;
            Lerp(t, duration);
        }

        private void RemoveLerp()
            => MovementController = null;

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            if ((MovementController as MovementControllerLerp) == null) return;
        }

        public override void Draw(Shader shader)
        {

            base.Draw(shader);
        }
    }
}
