using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render;
using Yahtzee.Render.Models;
using Yahtzee.Render.UI;

namespace Yahtzee.Game.Scenes
{
    class SceneLoading : Scene
    {

        private string[] _models = { "Basic/Cube.obj", "Scene/Tray/tray.obj", "Scene/Table/table.obj", "Scene/Window/wall.obj",
                                     "Scene/Window/windowframe.obj", "Scene/FullWall/fullWall.obj", "Scene/FullWall/fullWall.obj",
                                     "Scene/Candle/candle.obj", "Scene/Moon/plane.obj", "Tutorial/Tut1/plane.obj", "Tutorial/Tut2/plane.obj",
                                     "Scene/Flame/flame.obj"};

        private TextComponent _progressComponent;

        private int _index = 0;
        public override void Init()
        {
            base.Init();

            
            UI.AddComponent(new TextComponent(UI, Program.FontRepository.GetFont("arial.ttf", 65), "Loading...") { Alignment=TextAlignment.Centered});

            _progressComponent = new TextComponent(UI, Program.FontRepository.GetFont("arial.ttf", 35), "0/" + _models.Length + " models")
                            { Alignment = TextAlignment.Centered } ;
            _progressComponent.Transform.Translation += new GlmSharp.vec2(0, -100).ScaleToScreen();
            UI.AddComponent(_progressComponent);
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            ModelLoader.LoadModel(_models[_index++]);
            _progressComponent.Text = _index + "/" + _models.Length + " models";

            if (_index >= _models.Length) Program.SwitchScene(new SceneDiceRoll());
        }

    }
}
