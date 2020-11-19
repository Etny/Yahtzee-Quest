using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Physics;
using Yahtzee.Game.Physics;
using Yahtzee.Render;

namespace Yahtzee.Game.Entities
{
    class EntityStaticBody : ModelEntity
    {
        public RigidBody RigidBody { get { return ((MovementControllerRigidBody)MovementController).RigidBody; } }
        public bool Drawn { get; protected set; }

        public EntityStaticBody(string modelPath, bool drawn = true) : base(modelPath)
        {
            Drawn = drawn;
            if (!drawn) DrawInstanced = false;

            MovementController = new MovementControllerRigidBody(this);
            RigidBody.Static = true;
        }

        public override void Draw(Shader shader) { if (Drawn) base.Draw(shader); }
            

    }
}
