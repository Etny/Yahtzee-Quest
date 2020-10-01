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
        public bool DrawInstanced = true;

        public RigidBody RigidBody { get { return ((MovementControllerRigidBody)MovementController).RigidBody; } }
        public ModelEntity(string modelPath) : base() 
        {
            Model = ModelLoader.LoadModel(modelPath);
            ModelManager.Register(this, Model.Key);
            MovementController = new MovementControllerRigidBody(this); 
        }

        ~ModelEntity()
        {
            if (DrawInstanced) 
                ModelManager.Deregister(this, Model.Key);
        }

        public override void Draw(Shader shader)
        {
            //((MovementControllerRigidBody)MovementController).Collision.DrawOutline(Transform);
            //RigidBody.aabbMesh.DrawOutline(RigidBody.AABB.GetTransform());
            if (DrawInstanced) return;
            shader.SetMat4("models[0]", Transform.ModelMatrix);
            Model.Draw(shader);
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}
