using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game.Physics
{
    struct CollisionResult
    {
        public readonly RigidBody M1, M2;
        public readonly List<SupportPoint> Simplex;
        public readonly IEnumerable<vec3> SimplexPos { get { return Simplex.Select(p => p.Sup); } }

        public bool Colliding;

        public CollisionResult(RigidBody m1, RigidBody m2, List<SupportPoint> simplex, bool colliding = false)
        {
            M1 = m1;
            M2 = m2;
            Simplex = simplex;
            Colliding = colliding;
        }
    }
}
