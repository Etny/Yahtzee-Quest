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
        private Font _buttonFont;
        private ScoreSheet _sheet;

        private ButtonComponent _rerollButton;
        private TextComponent _rerollText;
        private TextComponent _rerollsLeftText;

        private List<EntityProxWall> _proxWalls = new List<EntityProxWall>();

        public SceneDiceRoll() : base() { }

        public override void Init()
        {
            base.Init();

            Sun = new DirectionalLight(new vec3(0, -1f, -.3f).NormalizedSafe);
            Sun.SetShadowsEnabled(true);
            Lights.Add(Sun);

            _font = Program.FontRepository.GetFont("orange_juice_2.ttf");
            _buttonFont = Program.FontRepository.GetFont("arial.ttf", 50 * Settings.ScreenRatio.x);
            _sheet = new ScoreSheet(UI, _font);

            _dice = new DiceManager(Gl);
            _dice.Populate(5);
            _dice.OnRolled += _sheet.UpdateRolled;

            var r = new ImageBlurRenderComponent(Gl, "Resource/Images/UI/Buttons/BlankButton.png");
            _rerollButton = new ButtonComponent(UI, ((vec2)r.Image.Size).ScaleToScreen() * .8f, r);
            _rerollButton.Transform.Translation += new vec2(-745, -150).ScaleToScreen();
            _rerollButton.Transform.Depth = .91f;
            UI.AddComponent(_rerollButton);
            _rerollButton.OnRelease += OnRerollPress;
 
            _rerollText = (TextComponent)UI.AddComponent(new TextComponent(UI, _buttonFont, "Reroll"));
            _rerollText.Alignment = TextAlignment.Centered;
            _rerollText.Transform.Translation = _rerollButton.Transform.Translation;
            _rerollText.Transform.Depth = .9f;

            _rerollsLeftText = (TextComponent)UI.AddComponent(new TextComponent(UI, _buttonFont, ""));
            _rerollsLeftText.Alignment = TextAlignment.Centered;
            _rerollsLeftText.Transform.Translation = _rerollButton.Transform.Translation - new vec2(0, 65);
            _rerollsLeftText.Transform.Scale = new vec2(.6f);
            _rerollsLeftText.Transform.Depth = .9f;

            _sheet.CanScore = _dice.CanScore;
            _sheet.OnSelect += onSheetSelect;
            _dice.OnPrepareRoll += _sheet.ClearFields;

            CurrentCamera.Transform.Translation = new vec3(0, 6.5f, 5.5f);
            CurrentCamera.Transform.RotateX(-45f.AsRad());


            CreateLevel();
        }

        private void onSheetSelect()
        {
            if (_sheet.AllFieldsLocked())
                Program.SwitchScene(new SceneFinalScore(_sheet.GetTotalScore()));
            else
                _dice.NewRoll();
        }

        private void CreateLevel()
        {
            void AddCube(bool drawn, bool wall, vec3 pos, vec3 scale)
            {
                var e = new EntityStaticBody("Basic/Cube.obj", drawn) { Position = pos };
                e.Transform.Scale = scale;
                Entities.Add(e);

                if (wall)
                {
                    e.RigidBody.Restitution = 7;
                    var w = new EntityProxWall(Gl, "Basic/Cube.obj", _dice.Dice) { Position = pos, Threshold = 1.7f};
                    w.Transform.Scale = scale;
                    _proxWalls.Add(w);
                }
            }

            AddCube(false, false, new vec3(0, -3, 0), new vec3(100, 1, 100));

            AddCube(false, true, new vec3(-65, 0, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(65, 0, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 0, -60), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 65, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 0, 53), new vec3(100, 100, 100));

            var e = new EntityStaticBody("Scene/Tray/tray.obj") { Position = new vec3(0, -2.3f, -3.5f) };
            e.Transform.Scale = new vec3(4, 1.3f, 1.733f);
            Entities.Add(e);
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            _dice.Update(deltaTime, !_sheet.Hovered);

            if (_dice.CanReroll())
            {
                if (_rerollText.Text == "") _rerollText.Text = "Reroll";
                if (_rerollText.Text == "Reroll" && _dice.Rolling.Count <= 0) _rerollText.Text = "Reroll All";
                    else if (_rerollText.Text == "Reroll All" && _dice.Rolling.Count > 0) _rerollText.Text = "Reroll";
            }
            else _rerollText.Text = "";

            _rerollsLeftText.Text = "Rerolls Left: " + _dice.RerollsLeft; 
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

        private void OnRerollPress(object sender, EventArgs e)
        {
            _dice.PrepareRoll();
        }
    }
}
