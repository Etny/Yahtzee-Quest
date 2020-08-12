using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Game.Physics;

namespace Yahtzee.Game
{
    class ModelEntity : Entity
    {

        public Model Model;

        public RigidBody RigidBody { get { return ((MovementControllerRigidBody)MovementController).RigidBody; } }
        public CollisionMesh Collision { get { return ((MovementControllerRigidBody)MovementController).Collision; } }
        public ModelEntity(string modelPath) : base() { Model = new Model(modelPath); MovementController = new MovementControllerRigidBody(this); }

        public override void Draw(Shader shader)
        { 
            shader.SetMat4("model", Transform.ModelMatrix);
            Model.Draw(shader);
            ((MovementControllerRigidBody)MovementController).Collision.DrawOutline();
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}
