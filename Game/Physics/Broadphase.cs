using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Yahtzee.Game.Physics
{
    class Broadphase
    {

        public static List<(RigidBody, RigidBody)> GetColliderPairs(ImmutableList<RigidBody> bodies)
        {
            List<(RigidBody, RigidBody)> pairs = new List<(RigidBody, RigidBody)>();

            for (int i = 0; i < bodies.Count - 1; i++)
            {
                RigidBody body1 = bodies[i];

                for (int j = i + 1; j < bodies.Count; j++)
                {
                    RigidBody body2 = bodies[j];

                    if (!body1.PhysicsActive && !body2.PhysicsActive) continue;

                    if (body1.AABB.Intersects(body2.AABB))
                    {
                        if (body1.Sleeping && !body1.AABBOverlapCache.Contains(body2.UID)) body1.WakeUp();
                        if (body2.Sleeping && !body2.AABBOverlapCache.Contains(body1.UID)) body2.WakeUp();
                        
                        body1.OverlappingAABB.Add(body2.UID);
                        body2.OverlappingAABB.Add(body1.UID);
                        pairs.Add((body1, body2));
                    }
                }
            }

                return pairs;
        }
    }
}
