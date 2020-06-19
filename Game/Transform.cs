using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game
{
    struct Transform
    {
        public vec3 Translation;
        public quat Rotation;
        public float Scale;

        public mat4 ModelMatrix { 
            get
            {
                return mat4.Translate(Translation) * Rotation.ToMat4 * mat4.Scale(Scale); 
            } 
        }

        public void Rotate(float angle, vec3 axis) => Rotation = Rotation.Rotated(angle, axis);

        public void RotateX(float angle) => Rotate(angle, vec3.UnitX);
        public void RotateY(float angle) => Rotate(angle, vec3.UnitY);
        public void RotateZ(float angle) => Rotate(angle, vec3.UnitZ);
    }
}
