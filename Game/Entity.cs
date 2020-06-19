using System;
using System.Collections.Generic;
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

        public Entity() { Transform = new Transform() { Translation = vec3.Zero, Rotation = quat.Identity, Scale = 1 }; }

        public virtual void Update(Time deltaTime) { }
        public virtual void Draw(Shader shader) { }

    }
}
