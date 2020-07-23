using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game.Physics
{
    struct CollisionResult
    {
        public readonly ModelEntity M1, M2;
        public readonly List<vec3> Simplex;

        public bool Colliding;

        public CollisionResult(ModelEntity m1, ModelEntity m2, List<vec3> simplex, bool colliding = false)
        {
            M1 = m1;
            M2 = m2;
            Simplex = simplex;
            Colliding = colliding;
        }
    }
}
