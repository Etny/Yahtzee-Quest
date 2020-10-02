using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game
{
    struct Transform
    {
        public static readonly Transform Identity = new Transform() { Translation = vec3.Zero, Orientation = quat.Identity, Scale = new vec3(1) };

        public vec3 Translation;
        public quat Orientation;
        public vec3 Scale;

        public mat4 ModelMatrix { get { return mat4.Translate(Translation) * Orientation.ToMat4 * mat4.Scale(Scale);} }

        public void Rotate(float angle, vec3 axis) => Orientation = Orientation.Rotated(angle, axis);

        public void Rotate(float angleX, float angleY, float angleZ) { RotateX(angleX); RotateY(angleY); RotateZ(angleZ); }
        public void RotateX(float angle) => Rotate(angle, vec3.UnitX);
        public void RotateY(float angle) => Rotate(angle, vec3.UnitY);
        public void RotateZ(float angle) => Rotate(angle, vec3.UnitZ);

        public vec3 Apply(vec3 v)
            => (ModelMatrix * new vec4(v, 1)).xyz;

        public static vec3 operator *(Transform t, vec3 rhs) => t.Apply(rhs);
        public static Transform operator *(Transform lhs, Transform rhs)
        {
            Transform t = Identity;
            t.Translation = lhs.Translation + rhs.Translation;
            t.Scale = lhs.Scale * rhs.Scale;
            t.Orientation = rhs.Orientation * lhs.Orientation;
            return t;
        }
    }
}
