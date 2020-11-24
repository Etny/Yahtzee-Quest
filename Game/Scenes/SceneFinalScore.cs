using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render;
using Yahtzee.Render.UI;

namespace Yahtzee.Game.Scenes
{
    class SceneFinalScore : Scene
    {

        private int _score;


        public SceneFinalScore(int score)
        {
            _score = score;
        }

        public override void Init()
        {
            base.Init();

            Font writingFont = Program.FontRepository.GetFont("orange_juice_2.ttf", 60 * Settings.ScreenRatio.x);

            UI.AddComponent(new TextComponent(UI, writingFont, "Final Score: " + _score));
        }

    }
}
