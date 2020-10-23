using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Render.UI
{
    class UILayer
    {
        public ImmutableList<IUIComponent> Components;

        public UILayer()
        {
            Components = ImmutableList.Create<IUIComponent>();

            Components = Components.Add(new QuadComponent());
        }


        public void Draw()
            => Components.ForEach(c => c.Draw());

        public void Update(Time deltaTime)
            => Components.ForEach(c => c.Update(deltaTime));
    }
}
