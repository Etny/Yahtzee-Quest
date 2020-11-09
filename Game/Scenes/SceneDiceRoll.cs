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

namespace Yahtzee.Game.Scenes
{
    class SceneDiceRoll : Scene
    {
        
        public SpotLight Flashlight;
        public DirectionalLight Sun;

        private DiceSet _dice;


        public SceneDiceRoll() : base() { }

        public override void Init()
        {
            base.Init();

            //flashLight = new SpotLight(new vec3(0, 3, -2), Util.ToRad(25), Util.ToRad(30)) { Direction = new vec3(0, -1, 1) };
            //Lights.Add(flashLight);
            Sun = new DirectionalLight(new vec3(.1f, -.5f, -.5f));
            Sun.SetShadowsEnabled(true);
            Lights.Add(Sun);

            var e = new EntityStaticBody("Basic/Cube.obj") { Position = new vec3(0f, -3f, 0) };
            e.Transform.Scale = new vec3(100, 1f, 100);
            Entities.Add(e);

            _dice = new DiceSet(Gl);
            _dice.Populate(5);
        }

        public override void Update(Time deltaTime)
        {
            base.Update(deltaTime);

            //Flashlight.SetPositionAndDirection(CurrentCamera.Position, CurrentCamera.GetDirection());
            _dice.Update(deltaTime);
        }

        protected override void RenderExtras(FrameBuffer frameBuffer)
        {
            _dice.Draw();
        }


        protected override void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            
            if (key == Keys.P && action == InputAction.Press)
            {
                vec3 o = new vec3(0, 4, 0);
                Random r = new Random();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        EntityDie m = new EntityDie("Dice/D6Red/d6red.obj") { Position = o + new vec3(i * 1.2f, 0, j * 1.2f) };
                        m.Transform.Rotate((float)r.NextDouble(), new vec3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));
                        Entities.Add(m);
                    }
                }
            }
            else if (key == Keys.O && action == InputAction.Press)
            {
                Random r = new Random();
                EntityDie m = new EntityDie("Dice/D6Red/d6red.obj") { Position = new vec3(0, 4, 0) };
                m.Transform.Rotate((float)r.NextDouble(), new vec3((float)r.NextDouble() * 3, (float)r.NextDouble() * 3, (float)r.NextDouble() * 3));
                Entities.Add(m);
            }
            else if (key == Keys.Q && action == InputAction.Press)
            {
                //_dice.Populate(5);
                _dice.Roll();
            }
        }
    }
}
