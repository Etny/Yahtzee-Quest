﻿using GlmSharp;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Curve;
using Yahtzee.Game.Entities;
using Yahtzee.Main;

namespace Yahtzee.Game
{
    class MovementControllerLerp : IMovementController
    {

        public Transform OldTransform { get; protected set; }
        public Transform TargetTransform { get; protected set; }

        public ICurve Curve { get; set; } = new LinearCurve();

        public float LerpTime { get; protected set; }
        public float LerpProgress = 0;

        public float Progress { get { return LerpProgress / LerpTime; } }

        public bool Lerping = false;

        public event Action OnComplete;

        public MovementControllerLerp(Transform old, Transform target, float speed = 1f)
        {
            OldTransform = old;
            TargetTransform = target;
            LerpTime = speed;
        }

        public void UpdateMovement(Time deltaTime, Entity e)
        {
            if (Lerping)
                if (LerpProgress < LerpTime)
                    LerpProgress += LerpTime - LerpProgress > deltaTime.DeltaF ? deltaTime.DeltaF : LerpTime - LerpProgress;
                else { OnComplete?.Invoke(); Lerping = false; }
            

            e.Transform = Transform.Lerp(OldTransform, TargetTransform, Progress, Curve);
        }
    }
}
