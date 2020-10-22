using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;
using GlmSharp;
using Yahtzee.Main;
using Yahtzee.Game.Physics;
using Yahtzee.Render;

namespace Yahtzee.Game
{
    class DiceSet
    {
        List<EntityDie> Dice = new List<EntityDie>();

        private int _sleepCount = 0;

        Shader HighlightShader;

        public DiceSet()
        {
            HighlightShader = Program.PostProcessManager.AddPostProcessShader("glow");
            HighlightShader.SetVec3("Color", new vec3(.8f, .3f, 0));
            HighlightShader.SetFloat("MaxDistance", 200);
        }

        public void Update(Time deltaTime)
        {
            if (Dice.Count <= 0) return;

            var camera = Program.Scene.CurrentCamera;

            float dot = -2;
            EntityDie closest = null;

            foreach (var die in Dice)
            {
                float d = vec3.Dot(camera.GetMouseRay(), (die.Position - camera.Position).NormalizedSafe);
                if (d < dot) continue;
                dot = d;
                closest = die;
            }

            var v1 = (camera.ProjectionMatrix * camera.ViewMatrix * closest.Transform.ModelMatrix * new vec4(vec3.Zero, 1));
            var v2 = v1.xy / v1.w;
            v2 = (v2 + vec2.Ones) / 2;
            Program.Window.GetSize(out int screenW, out int screenH);
            v2 *= new vec2(screenW, screenH);

            HighlightShader.SetVec2("GlowCenter", v2);
        }

        public void Populate(int count)
        {
            if (Dice.Count > 0) return;

            Random r = new Random();

            for (int i = 0; i < count; i++)
            {
                var d = new EntityDie("Dice/D6Red/d6red.obj") { Position = new vec3(-count + i * 2, 3, 0) };
                d.Transform.Rotate(2 * (float)r.NextDouble(), new vec3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()).NormalizedSafe);



                d.RigidBody.OnFallAsleep += Die_OnFallAsleep;

                Dice.Add(d);
            }

            HighlightShader.SetBool("Enabled", true);

            Program.Scene.Entities.AddRange(Dice);
        }

        private void Die_OnFallAsleep(object sender, EventArgs e)
        {
            RigidBody b = sender as RigidBody;
            EntityDie die = b.Parent as EntityDie;
            if (die == null) return;

            die.CalculateRolledIndex();

            
            die.CameraOffset = new vec3(-.8f + (.4f * _sleepCount), -.5f, -1f);
            _sleepCount++;
            if (_sleepCount == 3) die.Center = true;

            die.StartLerpToCamera();
        }
    }
}
