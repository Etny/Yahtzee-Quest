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
using System.Linq;

namespace Yahtzee.Game
{
    class DiceSet
    {
        public readonly List<EntityDie> Dice = new List<EntityDie>();

        private readonly Outliner outliner;
        private static readonly float _highlightCutoff = (float)Math.Cos(12f.AsRad());

        public bool Rolling { get; private set; } = false;

        public DiceSet(GL gl)
        {
            outliner = new Outliner(gl);
        }

        public void Update(Time deltaTime)
        {
            if (Dice.Count <= 0) return;

            CalculateHighlight();

            if (!Rolling) return;

            if(!Dice.Exists(d => !d.RigidBody.Sleeping))
            {
                MoveDiceToCamera();
                Rolling = false;
            }
        }

        private void CalculateHighlight()
        {
            var camera = Program.CurrentScene.CurrentCamera;

            float dot = -2;
            EntityDie closest = null;

            foreach (var die in Dice)
            {
                float d = vec3.Dot(camera.GetMouseRay(), (die.Position - camera.Position).NormalizedSafe);
                if (d < _highlightCutoff) continue;
                if (d < dot) continue;
                dot = d;
                closest = die;
            }

            outliner.Entity = closest;
            outliner.Enabled = true;
        }

        private void MoveDiceToCamera()
        {
            Dice.ForEach(d => d.CalculateRolledIndex());

            var ds = (from die in Dice orderby die.GetRolledNumber() select die).ToArray();

            for(int i = 0; i < ds.Length; i++)
            {
                var d = ds[i];

                d.LerpDuration = .6f + (i * .2f);
                d.CameraOffset = new vec3(-2.1f + (1.05f * i), -1.2f, -2.5f);
                d.StartLerpToCamera();
            }
        }

        public void Populate(int count)
        {
            if (Dice.Count > 0) return;

            Random r = new Random();

            for (int i = 0; i < count; i++)
            {
                var d = new EntityDie("Dice/D6Red/d6red.obj") { Position = new vec3(-count + i * 2, 3, 0) };
                d.Transform.Rotate(2 * (float)r.NextDouble(), new vec3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()).NormalizedSafe);

                Dice.Add(d);
            }


            Program.CurrentScene.Entities.AddRange(Dice);
        }

        public void Roll()
        {
            Dice.ForEach(d => d.EnablePhysics());
            Rolling = true;
        }

        public void Draw()
            => outliner.Draw();
    }
}
