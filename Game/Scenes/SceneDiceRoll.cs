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
using Yahtzee.Core.Curve;

namespace Yahtzee.Game.Scenes
{
    class SceneDiceRoll : Scene
    {
        
        public DirectionalLight Sun;

        private DiceManager _dice;
        private Font _font;
        private Font _buttonFont;
        private ScoreSheet _sheet;

        private ModelEntity _tut1, _tut2;

        private float _startDelay = 1.5f;

        private PointLight _candleLight;
        private Entity _candleParent;
        private ICurve _candleCurve = new BezierCurve(new vec2(.41f, .09f), new vec2(.56f, .88f));
        private vec3 _candleTargetPos;
        private vec3 _candleLightCenter;
        private ModelEntity _flame;

        private ButtonComponent _rerollButton;
        private TextComponent _rerollText;
        private TextComponent _rerollsLeftText;

        private List<EntityProxWall> _proxWalls = new List<EntityProxWall>();

        public SceneDiceRoll() : base() { }

        public override void Init()
        {
            base.Init();

            _font = Program.FontRepository.GetFont("orange_juice_2.ttf");
            _buttonFont = Program.FontRepository.GetFont("arial.ttf", 50 * Program.Settings.ScreenRatio.x * 1.2f);
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
            _rerollsLeftText.Transform.Translation = _rerollButton.Transform.Translation - new vec2(0, 120).ScaleToScreen();
            _rerollsLeftText.Transform.Scale = new vec2(.6f);
            _rerollsLeftText.Transform.Depth = .9f;

            _sheet.CanScore = _dice.CanScore;
            _sheet.OnSelect += onSheetSelect;
            _dice.OnPrepareRoll += OnPrepare;
            _dice.OnPrepareRoll += _sheet.ClearFields;
            _dice.OnRolled += OnRolled;

            CurrentCamera.Transform.Translation = new vec3(0, 7f, 8f);

            CreateLevel();

            _dice.PrepareRoll();
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
                    var w = new EntityProxWall(Gl, "Basic/Cube.obj", _dice.Dice) { Position = pos, Threshold = 1.7f };
                    w.Transform.Scale = scale;
                    _proxWalls.Add(w);
                }
            }

            AddCube(false, false, new vec3(0, -3, 0), new vec3(100, 1, 100));

            AddCube(false, true, new vec3(-61.5f, 0, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(61.5f, 0, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 0, -60), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 65, 0), new vec3(100, 100, 100));
            AddCube(false, true, new vec3(0, 0, 53), new vec3(100, 100, 100));

            ModelEntity AddModel(string modelName, vec3 pos, vec3 scale, float yRot)
            {
                var e = new ModelEntity(modelName) { Position = pos };
                e.Transform.Scale = scale;
                e.Transform.RotateY(yRot.AsRad());
                Entities.Add(e);
                return e;
            }

            AddModel("Scene/Tray/tray.obj", new vec3(0, -2.3f, -3.5f), new vec3(3, 1.3f, 1.733f), 0);
            AddModel("Scene/Table/table.obj", new vec3(0, -3, -5), new vec3(3.5f), 0);
            AddModel("Scene/Window/wall.obj", new vec3(0, 10, -30.2f), new vec3(3.5f), 90);
            AddModel("Scene/Window/windowframe.obj", new vec3(0, 10, -30), new vec3(3.5f), -90);
            AddModel("Scene/FullWall/fullWall.obj", new vec3(-30, 0, 0), new vec3(3.5f), 0);
            AddModel("Scene/FullWall/fullWall.obj", new vec3(30, 0, 0), new vec3(3.5f), -180);
            AddModel("Scene/Candle/candle.obj", new vec3(8, -4.2f, 5), new vec3(.7f), 0);
            AddModel("Scene/Moon/plane.obj", new vec3(3, 19, -40), new vec3(2), 0);
            
            _tut1 =  AddModel("Tutorial/Tut1/plane.obj", new vec3(_dice.CupPos + new vec3(6, 0, 0)), new vec3(3), 0);
            _tut2 = AddModel("Tutorial/Tut2/plane.obj", new vec3(new vec3(-3, 3, 3)), new vec3(3), 0);
            _tut1.Hide = true;
            _tut2.Hide = true;

            //AddModel("Scene/Nightsky/plane.obj", new vec3(3, 19, -45), new vec3(10), 0);

            _candleLight = new PointLight(new vec3(8, 4f, 5), .5f, .06f, .015f);
            _candleLight.Diffuse = new vec3(.85f, .53f, .1f);
            _candleLight.Specular = new vec3(.85f, .53f, .1f);
            _candleLight.Ambient = new vec3(.08f, .04f, .04f);
            _candleLight.SetShadowsEnabled(true);
            _candleLightCenter = _candleLight.Position;
            _candleTargetPos = _candleLightCenter;
            _candleParent = new EntityEmpty() { Position = _candleLightCenter };
            Entities.Add(_candleParent);
            Lights.Add(_candleLight);
            _flame = AddModel("Scene/Flame/flame.obj", new vec3(8, 2f, 5), new vec3(.35f), 0);
            _flame.CastShadow = false;

            var camLight = new PointLight(CurrentCamera.Position) { Constant = .8f, Linear = .7f };
            Lights.Add(camLight);
                
            Sun = new DirectionalLight(new vec3(0, -.85f, 1f).NormalizedSafe);
            Sun.Diffuse = new vec3(.05f);
            Sun.Specular = new vec3(1f);
            Sun.Ambient = new vec3(0f);
            Sun.SetShadowsEnabled(true);
            Lights.Add(Sun);

            var moonLamp = new SpotLight(new vec3(3, 19, -37), 25f.AsRad(), 30f.AsRad());
            moonLamp.Direction = new vec3(0, 0, -1);
            Lights.Add(moonLamp);

        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            if(_startDelay > 0)
            {
                _startDelay -= deltaTime.DeltaF;
                if(_startDelay <= 0)
                {

                    Transform targetTrans = CurrentCamera.Transform;
                    targetTrans.RotateX(-45f.AsRad());
                    CurrentCamera.MovementController = new MovementControllerLerp(CurrentCamera.Transform, targetTrans, 1.2f)
                    { Curve = new BezierCurve(new vec2(.5f, 0), new vec2(.5f, 1)), Lerping = true };
                    _tut1.Hide = false;

                }
            }

            _dice.Update(deltaTime, !_sheet.Hovered);

            if (_dice.CanReroll())
            {
                if (_rerollText.Text == "") _rerollText.Text = "Reroll";
                if (_rerollText.Text == "Reroll" && _dice.Rolling.Count <= 0) _rerollText.Text = "Reroll All";
                    else if (_rerollText.Text == "Reroll All" && _dice.Rolling.Count > 0) _rerollText.Text = "Reroll";
            }
            else _rerollText.Text = "";

            _rerollsLeftText.Text = "Rerolls Left: " + _dice.RerollsLeft;

            _candleLight.Position = _candleParent.Position;

            if (_candleParent.MovementController == null || ((MovementControllerLerp)_candleParent.MovementController).Progress >= 1) {
                var r = new Random();
                _candleTargetPos = _candleLightCenter + new vec3(((float)r.NextDouble() * .34f) - .17f, 0, ((float)r.NextDouble() * .34f) - .17f);
                Transform newTrans = new Transform() { Translation = _candleTargetPos };
                _candleParent.MovementController = new MovementControllerLerp(_candleParent.Transform, newTrans, .15f) { Curve = _candleCurve, Lerping  = true };
            }

            vec3 diff = (_flame.Transform.Translation - _candleParent.Transform.Translation).NormalizedSafe;
            vec3 cross = vec3.Cross(vec3.UnitY, diff);
            quat newQuat = new quat(cross.x, cross.y, cross.z, vec3.Dot(vec3.UnitY, diff));
            _flame.Transform.Orientation = newQuat;

            //_candleLight.Position = _candleLightCenter + new vec3(.3f * (float)(Math.Cos(5 * deltaTime.Total)), 0, .3f * (float)(Math.Sin(3 * deltaTime.Total)));
            //_dice.Dice[0].Position = _candleLight.Position;
        }

        protected override void RenderExtras(FrameBuffer frameBuffer)
        {
            _dice.Draw();
            _proxWalls.ForEach(p => p.Draw(null));
        }

        private void OnRolled(int[] unused)
        {
            if (Entities.Contains(_tut2) && _tut2.Hide)
            {
                _tut2.Transform.Orientation = (CurrentCamera.Transform.Orientation * _tut2.Transform.Orientation).NormalizedSafe;

                _tut2.Hide = false;
            }
        }
        private void OnPrepare()
        {
            if (_tut2 != null && !_tut2.Hide) { _tut2.Hide = true; Entities.Remove(_tut2); }
        }

        private void onSheetSelect()
        {
            if (_sheet.AllFieldsLocked())
                Program.SwitchScene(new SceneFinalScore(_sheet.GetTotalScore()));
            else
                _dice.NewRoll();
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
                if (!_tut1.Hide && action == InputAction.Release) _tut1.Hide = true;
            }
        }

        private void OnRerollPress(object sender, EventArgs e)
        {
            _dice.PrepareRoll();
        }
    }
}
