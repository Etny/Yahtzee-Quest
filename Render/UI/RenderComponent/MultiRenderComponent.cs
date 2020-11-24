using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Models;
using System.Linq;

namespace Yahtzee.Render.UI.RenderComponent
{
    class MultiRenderComponent : RenderComponent
    {
        public RenderComponent[] RenderComponents { get; protected set; }

        public MultiRenderComponent(params RenderComponent[] components)
        {
            RenderComponents = components;
        }

        public override void Draw(QuadComponent comp)
        {
            foreach (var r in RenderComponents)
                r.Draw(comp);
        }

        public T GetFirstOfType<T>() where T : RenderComponent
            => GetAllOfType<T>().First();

        public IEnumerable<T> GetAllOfType<T>() where T : RenderComponent
            => RenderComponents.Where(r => r is T).Select(r => r as T);
        
    }
}
