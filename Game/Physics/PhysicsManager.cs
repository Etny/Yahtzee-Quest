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

namespace Yahtzee.Game
{
    class PhysicsManager
    {
        public CollisionDetector Collisions;
        public PenetrationDepthDetector DepthDetector;

        private ImmutableList<RigidBody> Bodies;

        private static int maxIterations = 50;

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

            return;

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

            float[] ReactionMagnitude = new float[constraintsToSolve.Count];
            float[] d = new float[constraintsToSolve.Count];
            float[] n = new float[constraintsToSolve.Count];
            vec3[,] b = new vec3[Bodies.Count,2];

            for (int i = 0; i < constraintsToSolve.Count; i++)
            {
                ReactionMagnitude[i] = 0;
                var con = constraintsToSolve[i];
                var jac = con.Jacobian;

                //v.LengthSqr = Dot(v, v)
                d[i] = jac[0, 0].LengthSqr + jac[0, 1].LengthSqr + jac[1, 0].LengthSqr + jac[1, 1].LengthSqr;

                //if (d[i] <= 0) d[i] = 1;

                n[i] = con.GetEta(deltaTime);
                //Console.WriteLine(n[i]);

                b[con.M1.Index.Value, 0] += jac[0, 0] * ReactionMagnitude[i];
                b[con.M1.Index.Value, 1] += jac[0, 1] * ReactionMagnitude[i];

                b[con.M2.Index.Value, 0] += jac[1, 0] * ReactionMagnitude[i];
                b[con.M2.Index.Value, 1] += jac[1, 1] * ReactionMagnitude[i];
            }



            for (int iter = 0; iter < maxIterations; iter++)
            {
                if (constraintsToSolve.Count <= 0) break;

                for(int i = 0; i < constraintsToSolve.Count; i++)
                {
                    var con = constraintsToSolve[i];
                    vec3[,] Jacobian = con.Jacobian;
                    int i1 = con.M1.Index.Value, i2 = con.M2.Index.Value;

                    float deltaMag = (n[i] - vec3.Dot(Jacobian[0, 0], b[i1, 0]) - vec3.Dot(Jacobian[0, 1], b[i1, 1])
                                           - vec3.Dot(Jacobian[1, 0], b[i2, 0]) - vec3.Dot(Jacobian[1, 1], b[i2, 1])) / d[i];

                    db.Assert(deltaMag >= 0 || deltaMag < 0);

                    var oldMag = ReactionMagnitude[i];
                    ReactionMagnitude[i] = Math.Max(0f, oldMag + deltaMag);
                    deltaMag = ReactionMagnitude[i] - oldMag;

                    b[i1, 0] += deltaMag * Jacobian[0, 0];
                    b[i1, 1] += deltaMag * Jacobian[0, 1];
                    b[i2, 0] += deltaMag * Jacobian[1, 0];
                    b[i2, 1] += deltaMag * Jacobian[1, 1];
                }
            }

            for(int i = 0; i < constraintsToSolve.Count; i++)
            {
                var constraint = constraintsToSolve[i];
                vec3[,] Jacobian = constraint.Jacobian;
                var Magnitude = ReactionMagnitude[i];

                constraint.M1.ForcesConstraints += Jacobian[0, 0] * Magnitude;
                constraint.M1.TorqueConstraints += Jacobian[0, 1] * Magnitude;

                //Console.WriteLine(Magnitude);

                if (constraint.M2.Static) continue;
                constraint.M2.ForcesConstraints += Jacobian[1, 0] * Magnitude;
                constraint.M2.TorqueConstraints += Jacobian[1, 1] * Magnitude;
            }

            Bodies.ForEach(b => b.ApplyFinalForces(deltaTime));
        }

    }
}
