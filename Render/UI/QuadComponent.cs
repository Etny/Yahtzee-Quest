using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Core;
using Yahtzee.Game;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;
using Yahtzee.Render.UI.RenderComponent;

namespace Yahtzee.Render.UI
{
    class QuadComponent : IUIComponent
    {
        public Transform2D Transform = Transform2D.Identity;
        public vec2 Position { get { return Transform.Translation; } set { Transform.Translation = value; } }

        public QuadMesh Quad { get; protected set; }

        public RenderComponent.RenderComponent RenderComponent { get; }

        public readonly UILayer Layer;


        public QuadComponent(UILayer layer) { Layer = layer; RenderComponent = new BasicRenderComponent(); }

        public QuadComponent(UILayer layer, vec2 size) : this(layer)
        {
            Quad = new QuadMesh(size.ScaleToScreen());
        }

        public QuadComponent(UILayer layer, vec2 size, RenderComponent.RenderComponent component)
        {
            Layer = layer;
            Quad = new QuadMesh(size.ScaleToScreen());
            RenderComponent = component;
        }


        public virtual void Draw()
            => RenderComponent.Draw(this);


        public virtual void Update(Time deltaTime) { }
    }
}
