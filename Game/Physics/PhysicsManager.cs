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
                        body1.Collision.Overlapping = true;
                        body2.Collision.Overlapping = true;
                        body1.Overlapping.Add(body2.UID);
                        body2.Overlapping.Add(body1.UID);
                        constraintsToSolve.Add(new ConstraintCollision(result));

                        //var bs = body1.Static ? body1 : body2;
                        //var bp = body1.Static ? body2 : body1;
                        //float top = bs.Position.y + .5f;
                        //vec3 c1 = bp.Transform.Apply(new vec3(-.5f, -.5f, -.5f));
                        //vec3 c2 = bp.Transform.Apply(new vec3(.5f, -.5f, -.5f));
                        //vec3 c3 = bp.Transform.Apply(new vec3(-.5f, -.5f, .5f));
                        //vec3 c4 = bp.Transform.Apply(new vec3(.5f, -.5f, .5f));

                        //constraintsToSolve.Add(new ConstraintCollision(result, c1, new vec3(c1.x, top, c1.z)));
                        //constraintsToSolve.Add(new ConstraintCollision(result, c2, new vec3(c2.x, top, c2.z)));
                        //constraintsToSolve.Add(new ConstraintCollision(result, c3, new vec3(c3.x, top, c3.z)));
                        //constraintsToSolve.Add(new ConstraintCollision(result, c4, new vec3(c4.x, top, c4.z)));
                    }
                }
            }

            foreach (Constraint c in ConstraintCache)
                if (c.StillValid()) constraintsToSolve.Add(c);
            ConstraintCache.Clear();

            float[] ReactionMagnitude = new float[constraintsToSolve.Count];

            for (int i = 0; i < constraintsToSolve.Count; i++)
                ReactionMagnitude[i] = 0;

            foreach(var c in constraintsToSolve)
            {
                c.Resolve(deltaTime);
                c.Age++;
                if (c.StillValid()) ConstraintCache.Add(c);
                else Program.Scene.ContactPointVisualizer.RemovePoints(((ConstraintCollision)c).meshes);
            }

            Bodies.ForEach(b => b.ApplyFinalForces(deltaTime));
        }

    }
}
