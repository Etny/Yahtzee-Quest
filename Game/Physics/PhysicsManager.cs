using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using GlmSharp;
using Yahtzee.Main;
using System.Diagnostics;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Reflection.Metadata.Ecma335;
using SixLabors.Primitives;
using Yahtzee.Game.Physics;
using Silk.NET.Vulkan;
using System.Collections.Immutable;

namespace Yahtzee.Game
{
    class PhysicsManager
    {
        public CollisionDetector Collisions;
        public PenetrationDepthDetector DepthDetector;

        private ImmutableList<RigidBody> Bodies;

        public PhysicsManager()
        {
            Collisions = new CollisionDetector();
            DepthDetector = new PenetrationDepthDetector(this);

            Bodies = ImmutableList<RigidBody>.Empty;
        }

        public void RegisterRigidBody(RigidBody body)
        {
            if (Bodies.Contains(body)) return;

            Bodies = Bodies.Add(body);
            body.Index = Bodies.Count - 1;
        }

        public void DeregisterRigidBody(RigidBody body)
        {
            Bodies = Bodies.Remove(body);
            body.Index = null;

            for (int i = 0; i < Bodies.Count; i++)
                Bodies[i].Index = i;
        }

        public ImmutableList<RigidBody> GetPhysicsBodies()
            => Bodies;

        public void Update(Time deltaTime)
        {
            var constraintsToSolve = new List<CollisionResult>();

            Bodies.ForEach(b => b.ApplyInitialForces(deltaTime));

            for (int i = 0; i < Bodies.Count-1; i++)
            {
                RigidBody body1 = Bodies[i];

                for(int j = i + 1; j < Bodies.Count; j++)
                {
                    RigidBody body2 = Bodies[j];

                    if (body1.Static && body2.Static) continue;

                    CollisionResult result;
                    if(!body1.Static) 
                        result = Collisions.GJKResult(body1, body2);
                    else 
                        result = Collisions.GJKResult(body2, body1);

                    if (result.Colliding == true)
                    {
                        body1.Collision.Overlapping = true;
                        body2.Collision.Overlapping = true;
                        constraintsToSolve.Add(result);
                    }
                }
            }

            //TODO: Solve Constraints

            Bodies.ForEach(b => b.ApplyFinalForces(deltaTime));
        }
    }
}
