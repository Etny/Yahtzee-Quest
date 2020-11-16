using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Core
{
    struct Transform2D
    {

        public static readonly Transform2D Identity = new Transform2D() { Translation = vec2.Zero, Orientation = 0, Depth = .5f, Scale = vec2.Ones };

        public vec2 Translation;
        public float Orientation;
        public vec2 Scale;
        public float Depth;

        public mat4 ModelMatrix { get { return mat4.Translate(new vec3(Translation, 0)) * quat.FromAxisAngle(Orientation, vec3.UnitZ).ToMat4 * mat4.Scale(new vec3(Scale, 1)); } }
        public mat4 ModelMatrixUI { get { return mat4.Translate(new vec3(Translation * 2, 0)) * quat.FromAxisAngle(Orientation, vec3.UnitZ).ToMat4 * mat4.Scale(new vec3(Scale, 1)); } }


    }
}
