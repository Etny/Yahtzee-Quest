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
using Yahtzee.Core.Curve;

namespace Yahtzee.Game
{
    class DiceManager
    {
        public readonly List<EntityDie> Dice = new List<EntityDie>();

        private static readonly float _highlightCutoff = (float)Math.Cos(12f.AsRad());
        private readonly vec3 _hoverColor = new vec3(1);
        private readonly vec3 _rerollColor = new vec3(.7f, .3f, .1f);
        private readonly float _hoverThickness = .1f;
        private readonly float _rerollThickness = .07f;
        private EntityDie _hoveredDie;

        private int _rollsPerTry = 3;
        private int _currentRolls = 3;

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
        private readonly float _shakeScale = .008f;
        private readonly vec2 _maxShakeDist = new vec2(1920 / 2, 1080 / 2).ScaleToScreen();
        private readonly ICurve _shakeCurve = new BezierCurve(new vec2(0, .39f), new vec2(.14f, .74f));
        private readonly float _maxShakeMove = 3.2f;
        private vec3 _lastShakeMomentum;
        private vec2 _lastShakePos;
        private readonly float _shakeRotSpeed = 220f.AsRad();
        private quat _shakeRotAxis = quat.Identity;

        private GL _gl;

        public DiceManager(GL gl)
        {
            _gl = gl;
        }

        public void Update(Time deltaTime, bool useMouse = true)
        {
            if (Dice.Count <= 0) return;

            switch (State)
            {
                case RollState.Preparing:

                    if (_cupShakeCooldown > 0) { _cupShakeCooldown -= deltaTime.DeltaF; return; }

                    var shakeSpeed = _shakeRotSpeed + (_shaking ? (_lastShakeMomentum.Length + 1) : 0);

                    foreach (var r in Rolling)
                        r.Transform.Orientation = r.Transform.Orientation.Rotated(shakeSpeed * deltaTime.DeltaF, _shakeRotAxis * vec3.UnitY);

                    _shakeRotAxis = _shakeRotAxis.Rotated(shakeSpeed / 3 * deltaTime.DeltaF, vec3.UnitZ);

                    if (!_shaking) return;

                    var v2 = (Program.InputManager.MousePosition - _shakeCenter) / _maxShakeDist;
                    var signs = new vec2(Math.Sign(v2.x), Math.Sign(v2.y));
                    v2 = signs * v2;
                    v2 = _maxShakeMove * new vec2(signs.x * _shakeCurve[v2.x <= 1 ? v2.x : 1], signs.y * _shakeCurve[v2.y <= 1 ? v2.y : 1]);
                    _currentCupPos = CupPos + new vec3(v2.x, 0, v2.y);
                    v2 = new vec2(Program.InputManager.MousePosition - _lastShakePos) * _shakeScale;
                    _lastShakeMomentum = new vec3(v2.x, 0, v2.y) / deltaTime.DeltaF;
                    _lastShakePos = Program.InputManager.MousePosition;

                    foreach (var d in Rolling)
                        d.Transform.Translation = _currentCupPos + _cupOffset[d];

                    break;

                case RollState.Rolling:

                    if (!Rolling.Exists(d => !d.RigidBody.Sleeping))
                    {
                        MoveDiceToCamera();
                        State = RollState.Scoring;
                        if (Rolling.Count == Dice.Count) { Rolling.Clear(); UpdateOutliners(); }
                        OnRolled?.Invoke(Rolled);
                    }

                    break;

                case RollState.Scoring:

                    if (useMouse) UpdateOutliners();

                    break;
            }
        }

        private void UpdateOutliners()
        {
            var camera = Program.CurrentScene.CurrentCamera;

            float dot = -2;
            _hoveredDie = null;


            foreach (var die in Dice)
            {
                float d = vec3.Dot(camera.GetMouseRay(), (die.Position - camera.Position).NormalizedSafe);
                if (d < _highlightCutoff) continue;
                if (d < dot) continue;
                dot = d;
                _hoveredDie = die;
            }

            foreach(var d in Dice)
            {
                if (d == _hoveredDie) {
                    d.Outliner.Enabled = true;
                    d.Outliner.Color = Rolling.Contains(d) ? vec3.Lerp(_hoverColor, _rerollColor, .5f) : _hoverColor;
                    d.Outliner.OutlineSize = _hoverThickness;
                } 
                else if (!Rolling.Contains(d))
                {
                    d.Outliner.Enabled = false;
                }
                else
                {
                    d.Outliner.OutlineSize = _rerollThickness;
                    d.Outliner.Color = _rerollColor;
                    d.Outliner.Enabled = true;
                }
            }
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
                d.CameraOffset = new vec3(-2.1f + (1.05f * i), -1.4f, -2.5f);
                d.StartLerpToCamera();
            }
        }

        public void PrepareRoll()
        {
            if (State != RollState.Scoring) return;
            if (Rolling.Count == 0) return;
            if (_currentRolls <= 0) return;
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
                var d = new EntityDie(_gl, "Dice/D6Red/d6red.obj") { Position = new vec3(-count + i * 2, 3, 0) };
                d.Transform.Rotate(2 * (float)r.NextDouble(), new vec3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()).NormalizedSafe);

                Dice.Add(d);
            }

            Rolling = Dice.GetRange(0, count);
            Program.CurrentScene.Entities.AddRange(Dice);
        }

        public void NewRoll()
        {
            _currentRolls = _rollsPerTry;
            Rolled = new int[] { 0, 0, -3, -2, -1 };
            Rolling = Dice.GetRange(0, Dice.Count);
            PrepareRoll();
        }

        public void Roll()
        {
            if (State != RollState.Preparing) return;
            State = RollState.Rolling;
            _currentRolls--;

            var momentum = _lastShakeMomentum;
            var shakeSpeed = _shakeRotSpeed + (_lastShakeMomentum.Length + 1);
            var rotAxis = (_shakeRotAxis * vec3.UnitY).NormalizedSafe;

            foreach (var d in Rolling)
            {
                d.EnablePhysics();
                d.RigidBody.Impulse(momentum);
                d.RigidBody.AngularVelocity += (d.Transform.Orientation * rotAxis).NormalizedSafe * shakeSpeed;
            }
        }

        public void MouseButton(bool pressed)
        {
            switch (State)
            {
                case RollState.Preparing:
                    Shake(pressed);
                    break;

                case RollState.Scoring:
                    if (_hoveredDie == null || pressed) return;
                    if (Rolling.Contains(_hoveredDie)) Rolling.Remove(_hoveredDie);
                    else Rolling.Add(_hoveredDie);
                    break;
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

        public bool CanScore()
            => State == RollState.Scoring && Rolled != null;

        public void Draw()
        {
            if (State == RollState.Scoring)
            {
                Dice.FindAll(d => d.Outliner.Enabled && d != _hoveredDie).ForEach(d => d.Outliner.Draw(false));
                Dice.FindAll(d => d == _hoveredDie).ForEach(d => d.Outliner.Draw(true));
            }
        }
        private enum RollState { Rolling, Preparing, Scoring }
    }
}
