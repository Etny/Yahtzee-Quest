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
    class DiceManager
    {
        public readonly List<EntityDie> Dice = new List<EntityDie>();

        private readonly Outliner outliner;
        private static readonly float _highlightCutoff = (float)Math.Cos(12f.AsRad());

        private RollState State = RollState.Scoring;
        private List<EntityDie> Rolling = new List<EntityDie>();
        public int[] Rolled;
        public event Action<int[]> OnRolled;

        public vec3 CupPos = new vec3(0, 1.5f, -2);
        private vec3 _currentCupPos;
        private Dictionary<EntityDie, vec3> _cupOffset = new Dictionary<EntityDie, vec3>();
        private readonly float _cupLerpSpeed = .8f;
        private readonly float _cupSpreadDist = 1.3f;
        private float _cupShakeCooldown = 0;
        private bool _shaking = false;
        private vec2 _shakeCenter;
        private readonly float _shakeScale = .01f;
        private vec3 _lastShakeMomentum;
        private vec2 _lastShakePos;

        public DiceManager(GL gl)
        {
            outliner = new Outliner(gl);
        }

        public void Update(Time deltaTime, bool useMouse = true)
        {
            if (Dice.Count <= 0) return;

            switch (State)
            {
                case RollState.Preparing:

                    if(_cupShakeCooldown > 0) { _cupShakeCooldown -= deltaTime.DeltaF; return; }

                    if (!_shaking) return;

                    _currentCupPos = CupPos + new vec3(new vec2(Program.InputManager.MousePosition - _shakeCenter) * new vec2(_shakeScale, -_shakeScale), 0);
                    _lastShakeMomentum = new vec3(new vec2(Program.InputManager.MousePosition - _lastShakePos) * new vec2(_shakeScale, -_shakeScale), 0) / deltaTime.DeltaF;
                    _lastShakePos = Program.InputManager.MousePosition;

                    foreach (var d in Rolling)
                        d.Transform.Translation = _currentCupPos + _cupOffset[d];

                    break;

                case RollState.Rolling:

                    if (!Rolling.Exists(d => !d.RigidBody.Sleeping))
                    {
                        MoveDiceToCamera();
                        State = RollState.Scoring;
                        OnRolled?.Invoke(Rolled);
                    }

                    break;

                case RollState.Scoring:

                    if (useMouse) CalculateHighlight();
                    else outliner.Enabled = false;

                    break;
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
            Rolling.ForEach(d => d.CalculateRolledIndex());

            var ds = (from die in Dice orderby die.GetRolledNumber() select die).ToArray();
            Rolled = (from d in ds select d.GetRolledNumber()).ToArray();

            for(int i = 0; i < ds.Length; i++)
            {
                var d = ds[i];

                d.LerpDuration = .6f + (i * .15f);
                d.CameraOffset = new vec3(-2.1f + (1.05f * i), -1.2f, -2.5f);
                d.StartLerpToCamera();
            }
            Rolling = Dice.GetRange(0, 3);
        }

        public void PrepareRoll()
        {
            if (State != RollState.Scoring) return;
            State = RollState.Preparing;
            _currentCupPos = CupPos;
            _cupShakeCooldown = _cupLerpSpeed;

            var boxSize = new vec3(1.2f/2);
            List<BoundingBox> boxes = new List<BoundingBox>();

            Random r = new Random();

            for(int i = 0; i < Rolling.Count; i++)
            {
                BoundingBox box;
                vec3 pos;
                quat q;

                do
                {
                    q = quat.Identity.Rotated(((float)r.NextDouble() * 360).AsRad(), vec3.UnitX)
                                     .Rotated(((float)r.NextDouble() * 360).AsRad(), vec3.UnitY)
                                     .Rotated(((float)r.NextDouble() * 360).AsRad(), vec3.UnitZ)
                                     .NormalizedSafe;
                    pos = ((vec3.UnitY * _cupSpreadDist) * q);
                    box = new BoundingBox(pos - boxSize, pos + boxSize);
                } while (boxes.Exists(b => b.Intersects(box)));

                Transform t = Transform.Identity;
                t.Translation = CupPos + pos;
                t.Orientation = q;
                Rolling[i].Lerp(t, _cupLerpSpeed);
                _cupOffset[Rolling[i]] = pos; 
                boxes.Add(box);
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

            Rolling = Dice;
            Program.CurrentScene.Entities.AddRange(Dice);
        }

        public void Roll()
        {
            if (State != RollState.Preparing) return;
            State = RollState.Rolling;

            var momentum = _lastShakeMomentum; 

            foreach (var d in Rolling)
            {
                d.EnablePhysics();
                d.RigidBody.Impulse(momentum);
            }
        }
        
        public void Shake(bool pressed)
        {
            if (State != RollState.Preparing) return;
            if (pressed == _shaking) return;
            _shaking = pressed;

            if (pressed)
            {
                _shakeCenter = Program.InputManager.MousePosition;
                _lastShakePos = _shakeCenter;
                _lastShakeMomentum = vec3.Zero;
            }
            else
                Roll();
        }

        public void Draw()
            => outliner.Draw();

        private enum RollState { Rolling, Preparing, Scoring }
    }
}
