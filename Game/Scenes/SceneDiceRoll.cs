using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Render.Textures;
using Silk.NET.GLFW;
using Yahtzee.Game.Entities;
using Yahtzee.Render.Models;
using Yahtzee.Render.UI;
using Yahtzee.Core.Debug;
using Yahtzee.Core;
using Yahtzee.Render;
using Yahtzee.Render.UI.RenderComponent;
using Yahtzee.Core.Font;
using System.Linq;

namespace Yahtzee.Game.Scenes
{
    class SceneDiceRoll : Scene
    {
        
        public DirectionalLight Sun;

        private DiceManager _dice;
        private Font _font;
        private ScoreSheet _sheet;

        private List<EntityProxWall> _proxWalls = new List<EntityProxWall>();

        public SceneDiceRoll() : base() { }

        public override void Init()
        {
            base.Init();

            Sun = new DirectionalLight(new vec3(0, -1f, -.3f).NormalizedSafe);
            Sun.SetShadowsEnabled(true);
            Lights.Add(Sun);

            _font = Program.FontRepository.GetFont("orange_juice_2.ttf");
            _sheet = new ScoreSheet(UI, _font);

            _dice = new DiceManager(Gl);
            _dice.Populate(5);
            _dice.OnRolled += _sheet.UpdateRolled;

            _sheet.CanScore = _dice.CanScore;
            _sheet.OnSelect += _dice.NewRoll;

            CurrentCamera.Transform.Translation = new vec3(0, 6.5f, 3.5f);
            CurrentCamera.Transform.RotateX(-45f.AsRad());


            CreateLevel();
        }

        private void CreateLevel()
        {
            void AddCube(bool drawn, vec3 pos, vec3 scale)
            {
                var e = new EntityStaticBody("Basic/Cube.obj", drawn) { Position = pos };
                e.Transform.Scale = scale;
                Entities.Add(e);

                if (!drawn)
                {
                    var w = new EntityProxWall(Gl, "Basic/Cube.obj", _dice.Dice) { Position = pos };
                    w.Transform.Scale = scale;
                    _proxWalls.Add(w);
                }
            }

            AddCube(true, new vec3(0, -3, 0), new vec3(100, 1, 100));

            AddCube(false, new vec3(-65, 0, 0), new vec3(100, 100, 100));
            AddCube(false, new vec3(65, 0, 0), new vec3(100, 100, 100));
            AddCube(false, new vec3(0, 0, -60), new vec3(100, 100, 100));
            AddCube(false, new vec3(0, 60, 0), new vec3(100, 100, 100));
            AddCube(false, new vec3(0, 0, 53), new vec3(100, 100, 100));
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            _dice.Update(deltaTime, !_sheet.Hovered);
        }

        protected override void RenderExtras(FrameBuffer frameBuffer)
        {
            _dice.Draw();
            _proxWalls.ForEach(p => p.Draw(null));
        }


        protected override void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            
            if (key == Keys.Q && action == InputAction.Press)
            {
                _dice.Roll();
            }
            else if (key == Keys.R && action == InputAction.Press)
            {
                _dice.PrepareRoll();
            }
        }

        protected override void OnMouseButton(MouseButton button, InputAction action, KeyModifiers mods)
        {
            if(button == MouseButton.Left && action != InputAction.Repeat)
            {
                _dice.MouseButton(action == InputAction.Press);
            }
        }
    }
}
