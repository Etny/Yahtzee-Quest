using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game
{
    struct Transform
    {
        public static Transform Identity = new Transform() { Translation = vec3.Zero, Rotation = quat.Identity, Scale = new vec3(1) };

        public vec3 Translation;
        public quat Rotation;
        public vec3 Scale;

        public mat4 ModelMatrix { get { return mat4.Translate(Translation) * Rotation.ToMat4 * mat4.Scale(Scale); } }

        public void Rotate(float angle, vec3 axis) => Rotation = Rotation.Rotated(angle, axis);

        public void RotateX(float angle) => Rotate(angle, vec3.UnitX);
        public void RotateY(float angle) => Rotate(angle, vec3.UnitY);
        public void RotateZ(float angle) => Rotate(angle, vec3.UnitZ);


        public vec3 Apply(vec3 v)
            => (ModelMatrix * new vec4(v, 1)).xyz;
    }
}
