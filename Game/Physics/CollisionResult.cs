using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlmSharp;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Yahtzee.Main;
using db = System.Diagnostics.Debug;

namespace Yahtzee.Game.Physics
{
    class CollisionResult
    {
        public readonly RigidBody Body1, Body2;
        public readonly List<SupportPoint> Simplex;

        public IEnumerable<vec3> SimplexPos { get { return Simplex.Select(p => p.Sup); } }

        public bool Colliding;

        public CollisionResult(RigidBody body1, RigidBody body2, List<SupportPoint> simplex, bool colliding = false)
        {
            Body1 = body1;
            Body2 = body2;
            Simplex = simplex;
            Colliding = colliding;
        }

    }
}
