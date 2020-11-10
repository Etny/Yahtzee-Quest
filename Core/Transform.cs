using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Core.Curve;

namespace Yahtzee.Core
{
    struct Transform
    {
        public static readonly Transform Identity = new Transform() { Translation = vec3.Zero, Orientation = quat.Identity, Scale = new vec3(1) };

        public vec3 Translation;
        public quat Orientation;
        public vec3 Scale;

        public mat4 ModelMatrix { get { return mat4.Translate(Translation) * Orientation.ToMat4 * mat4.Scale(Scale); } }

        public void Rotate(float angle, vec3 axis) => Orientation = Orientation.Rotated(angle, axis);

        public void Rotate(float angleX, float angleY, float angleZ) { RotateX(angleX); RotateY(angleY); RotateZ(angleZ); }
        public void RotateX(float angle) => Rotate(angle, vec3.UnitX);
        public void RotateY(float angle) => Rotate(angle, vec3.UnitY);
        public void RotateZ(float angle) => Rotate(angle, vec3.UnitZ);

        public vec3 Apply(vec3 v)
            => Apply(new vec4(v, 1)).xyz;

        public vec4 Apply(vec4 v)
            => ModelMatrix * v;

        public static Transform Lerp(Transform A, Transform B, float Ratio)
            => Lerp(A, B, Ratio, new LinearCurve());

        public static Transform Lerp(Transform A, Transform B, float Ratio, ICurve curve)
        {
            Transform t = Identity;

            t.Translation = vec3.Lerp(A.Translation, B.Translation, curve[Ratio]);
            t.Orientation = quat.Lerp(A.Orientation, B.Orientation, curve[Ratio]).NormalizedSafe;
            t.Scale = vec3.Lerp(A.Scale, B.Scale, curve[Ratio]);

            return t;
        }

        public static vec3 operator *(Transform t, vec3 rhs) => t.Apply(rhs);
        public static vec4 operator *(Transform t, vec4 rhs) => t.Apply(rhs);

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
