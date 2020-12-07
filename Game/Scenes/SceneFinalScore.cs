using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render;
using Yahtzee.Render.UI;
using GlmSharp;
using Yahtzee.Core;
using Silk.NET.GLFW;

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

            Font writingFont = Program.FontRepository.GetFont("orange_juice_2.ttf", 80 * Program.Settings.ScreenRatio.x);

            UI.AddComponent(new TextComponent(UI, writingFont, "Final Score: " + _score) { Alignment = TextAlignment.Centered });

            writingFont.Size = 50 * Program.Settings.ScreenRatio.x;
            var sorryText = new TextComponent(UI, writingFont, "Sorry, no fancy end screen (ran out of ideas)") { Alignment = TextAlignment.Centered };
            sorryText.Transform.Translation += new vec2(0, -175).ScaleToScreen();
            UI.AddComponent(sorryText);

            var clickText = new TextComponent(UI, writingFont, "Press 'R' to play again") { Alignment = TextAlignment.Centered };
            clickText.Transform.Translation += new vec2(0, -350).ScaleToScreen();
            UI.AddComponent(clickText);
        }

        protected override void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.R && action == InputAction.Release)
                Program.SwitchScene(new SceneDiceRoll());
        }
    }
}
