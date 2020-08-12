using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Game.Physics
{
    struct SupportPoint
    {
        public vec3 A, B;
        public vec3 Sup;

        public SupportPoint(vec3 A, vec3 B)
        {
            this.A = A;
            this.B = B;
            this.Sup = A - B;
        }

    }
}
