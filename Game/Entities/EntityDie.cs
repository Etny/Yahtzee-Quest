using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Physics;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Entities
{
    class EntityDie : ModelEntity
    {

        public bool UsingRigidBody { get { return MovementController is MovementControllerRigidBody; } }
        public RigidBody RigidBody { get { return ((MovementControllerRigidBody)MovementController).RigidBody; } }

        public EntityDie(string modelPath) : base(modelPath)
        {
            MovementController = new MovementControllerRigidBody(this);
            RigidBody.CollisionTransform.Scale = new vec3(.97f);

            RigidBody.OnFallAsleep += RigidBody_OnFallAsleep;
        }

        private void RigidBody_OnFallAsleep(object sender, EventArgs e)
        {
            var dirs = new vec3[]{vec3.UnitY, -vec3.UnitY, vec3.UnitX,
                                  -vec3.UnitX, vec3.UnitZ, -vec3.UnitZ};

            var nums = new int[] { 2, 4, 6, 5, 3, 1 };

            float dot = 0;
            int num = 0;

            for(int i = 0; i < 6; i++)
            {
                float d = vec3.Dot(Transform.Orientation * dirs[i], vec3.UnitY);
                if (i != 0 && d < dot) continue;
                dot = d;
                num = nums[i];
            }

            Console.WriteLine(num);
        }

        public override void Draw(Shader shader)
        {
            //RigidBody.aabbMesh.DrawOutline(RigidBody.AABB.GetTransform());
            ((MovementControllerRigidBody)MovementController).Collision.DrawOutline(RigidBody.Transform);
            base.Draw(shader);
        }
    }
}
