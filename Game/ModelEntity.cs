using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using Yahtzee.Main;
using GlmSharp;

namespace Yahtzee.Game
{
    class ModelEntity : Entity
    {

        public Model Model;
        public CollisionMesh collision;
        public ModelEntity(string modelPath) : base() { Model = new Model(modelPath); collision = Model.LoadCollisionMesh("Basic/CUbe.obj", this); }

        public override void Draw(Shader shader)
        {
            foreach(Entity e in Program.Scene.Entities)
            {
                if (e == this) continue;
                if (!(e is ModelEntity)) continue;
                collision.CheckCollision(((ModelEntity)e).collision);
            }

            shader.SetMat4("model", Transform.ModelMatrix);
            //Model.Draw(shader);
            collision.DrawOutline();
        }
    }
}
