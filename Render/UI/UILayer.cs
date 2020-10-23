using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Yahtzee.Render.UI
{
    class UILayer
    {
        public ImmutableList<UIComponent> Components;

        public UILayer()
        {
            Components = ImmutableList.Create<UIComponent>();


        }


        public void Draw()
            => Components.ForEach(c => c.Draw());
    }
}
