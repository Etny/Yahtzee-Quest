using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;
using GlmSharp;
using Yahtzee.Main;
using Yahtzee.Game.Physics;

namespace Yahtzee.Game
{
    class DiceSet
    {
        List<EntityDie> Dice = new List<EntityDie>();

        private int _sleepCount = 0;

        public DiceSet()
        {
            
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
