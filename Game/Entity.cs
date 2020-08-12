using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using GlmSharp;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game
{
    abstract class Entity
    {
        public Transform Transform;

        public vec3 Position { get { return Transform.Translation; } set { Transform.Translation = value; } }

        public IMovementController MovementController = null;

        public Entity() { Transform = new Transform() { Translation = vec3.Zero, Rotation = quat.Identity, Scale = new vec3(1) }; }

        public virtual void Update(Time deltaTime) 
        {
            if (MovementController != null)
                MovementController.UpdateMovement(deltaTime, this);
        }
        public virtual void Draw(Shader shader) { }

    }
}
