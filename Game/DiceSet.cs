using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;
using GlmSharp;
using Yahtzee.Main;
using Yahtzee.Render;
using Yahtzee.Core;
using Yahtzee.Core.Physics;
using Silk.NET.OpenGL;

namespace Yahtzee.Game
{
    class DiceSet
    {
        public readonly List<EntityDie> Dice = new List<EntityDie>();

        private int _sleepCount = 0;
        private readonly Outliner outliner;
        private static readonly float _highlightCutoff = (float)Math.Cos(12f.AsRad());

        public DiceSet(GL gl)
        {

            outliner = new Outliner(gl);
        }

        public void Update(Time deltaTime)
        {
            if (Dice.Count <= 0) return;

            var camera = Program.CurrentScene.CurrentCamera;

            float dot = -2;
            EntityDie closest = null;

            foreach (var die in Dice)
            {
                float d = vec3.Dot(camera.GetMouseRay(), (die.Position - camera.Position).NormalizedSafe);
                if (d < _highlightCutoff) continue;
                if (d < dot ) continue;
                dot = d;
                closest = die;
            }

            outliner.Entity = closest;
            outliner.Enabled = true;
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


            Program.CurrentScene.Entities.AddRange(Dice);
        }

        public void Roll()
        {
            Dice.ForEach(d => d.EnablePhysics());
        }

        public void Draw()
            => outliner.Draw();

        private void Die_OnFallAsleep(object sender, EventArgs e)
        {
            RigidBody b = sender as RigidBody;
            if (!(b.Parent is EntityDie die)) return;

            die.CalculateRolledIndex();

            die.CameraOffset = new vec3(-2.1f + (1.05f * _sleepCount), -1.2f, -2.5f);
            _sleepCount++;

            die.StartLerpToCamera();
        }
    }
}
