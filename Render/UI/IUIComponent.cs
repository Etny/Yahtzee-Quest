using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;

namespace Yahtzee.Render.UI
{
    interface IUIComponent
    {

        void Update(Time deltaTime);
        void Draw();
    }
}
