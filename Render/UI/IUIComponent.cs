using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;

namespace Yahtzee.Render.UI
{
    interface IUIComponent
    {

        public abstract void Update(Time deltaTime);
        public abstract void Draw();
    }
}
