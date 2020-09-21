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
using Microsoft.Extensions.DependencyModel;
using db = System.Diagnostics.Debug;
using Yahtzee.Game.Physics.Constraints;

namespace Yahtzee.Game
{
    class PhysicsManager
    {
        public CollisionDetector Collisions;
        public PenetrationDepthDetector DepthDetector;

        private ImmutableList<RigidBody> Bodies;
        private List<Constraint> ConstraintCache;

        private static int maxIterations = 50;
        private static int nextID = 0;

        public PhysicsManager()
        {
            Collisions = new CollisionDetector();
            DepthDetector = new PenetrationDepthDetector(this);

            Bodies = ImmutableList<RigidBody>.Empty;
            ConstraintCache = new List<Constraint>();
        }

        public void RegisterRigidBody(RigidBody body)
        {
            if (Bodies.Contains(body)) return;

            Bodies = Bodies.Add(body);
            body.Index = Bodies.Count - 1;
            body.UID = nextID++;
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
            var constraintsToSolve = new List<Constraint>();
            //if (ConstraintCache.Count > 0) Console.WriteLine(ConstraintCache.Count);
            //return;

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


                    if (result.Colliding)
                    {
                        body1.Overlapping.Add(body2.UID);
                        body2.Overlapping.Add(body1.UID);

                        var col = new ConstraintCollision(result);
                        constraintsToSolve.Add(col);                        
                    }
                }
            }

            foreach (Constraint c in ConstraintCache) constraintsToSolve.Add(c);
            ConstraintCache.Clear();

            for (int i = 0; i < 1; i++)
                constraintsToSolve.ForEach(c => c.Resolve(deltaTime));

            foreach(Constraint c in constraintsToSolve)
            {
                c.Age++;
                if (c.StillValid()) ConstraintCache.Add(c); 
                else Program.Scene.ContactPointVisualizer.RemovePoints(((ConstraintCollision)c).meshes); 
            }

            ConstraintCache.Clear();

            Bodies.ForEach(b => b.ApplyFinalForces(deltaTime));
        }

    }
}
