using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GlmSharp;
using Silk.NET.OpenGL;
using Yahtzee.Game.Debug;
using Yahtzee.Render;

namespace Yahtzee.Game
{
    /// <summary>
    /// Displays the inner workings of the Collision Detection function using meshes. Used for debugging.
    /// </summary>
    unsafe class CollisionDetectionVisualizer
    {
        private readonly ModelEntity m1, m2;
        private readonly PhysicsManager pm;

        public List<vec3> Simplex = new List<vec3>();
        public vec3 Direction;

        private LineMesh pointMesh;
        private LineMesh dirMesh;

        private vec3[] simplexColors = { new vec3(0.458f, 0.658f, 0.960f), new vec3(0.874f, 0.047f, 0.047f), new vec3(0.047f, 0.929f, 0.247f), new vec3(0.654f, 0.011f, 0.749f) };
        private vec3[] directionColors = { new vec3(0.682f, 0.035f, 0.262f), new vec3(0.682f, 0.647f, 0.035f) };

        private int counter = 0;

        private string[] messages = { "Intialize", "New Point", "Check for fail and do Simplex", "New Direction" };

        public CollisionDetectionVisualizer(ModelEntity m1, ModelEntity m2, PhysicsManager pm)
        {
            this.m1 = m1;
            this.m2 = m2;
            this.pm = pm;

            pointMesh = new LineMesh(null, simplexColors);
            dirMesh = new LineMesh(null, directionColors);
        }

        [Conditional("DEBUG")]
        public void UpdateGJK()
        {
            Console.WriteLine(messages[counter]);

            int r = pm.Collisions.GJK_Step(m1, m2, Simplex, ref Direction, ref counter);

            if (counter == 1) Console.WriteLine($"Direction: {Direction}");

            if (r == 0)
            {
                pointMesh.SetPoints(Simplex.ToArray());
                dirMesh.SetPoints(new vec3[]{ vec3.Zero, Direction.Normalized});
            }
            else
            {
                Simplex.Clear();
                counter = 0;
                Console.WriteLine(r == 1 ? "Collision!" : "Failed!");
                m1.collision.Overlapping = (r == 1);
            }
        }
        public void Draw()
        {
            if (Simplex.Count <= 0) return;

            pointMesh.Draw(null);
            dirMesh.Draw(null);
        }

    }
}
