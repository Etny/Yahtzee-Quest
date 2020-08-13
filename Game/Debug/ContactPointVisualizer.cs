using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assimp;
using GlmSharp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.Formats.Jpeg;
using Yahtzee.Game.Debug;
using Yahtzee.Game.Physics;
using Yahtzee.Render;
using static Yahtzee.Game.Physics.PenetrationDepthDetector;

namespace Yahtzee.Game
{
    /// <summary>
    /// Displays the inner workings of the Penetration Depth function using meshes. Used for debugging.
    /// </summary>
    unsafe class ContactPointVisualizer
    {
        public CollisionResult result;
        private readonly PhysicsManager pm;

        private vec3[] pointColor = { new vec3(0.807f, 0.070f, 0.792f) };
        LineMesh pointMesh;
        vec3 point = vec3.NaN;

        public ContactPointVisualizer(CollisionResult result, PhysicsManager pm)
        {
            this.result = result;
            this.pm = pm;

            pointMesh = new LineMesh(null, pointColor);
        }

        [Conditional("DEBUG")]
        public void UpdateContactPoints()
        {
            var p = pm.DepthDetector.Contact(result);

            if (p == vec3.NaN) return;

            point = p;

            Console.WriteLine($"Contact Point(s): {point}");

            pointMesh.SetPoints(new vec3[] { point });
        }
        public void Draw()
        {
            if (point == vec3.NaN) return;

            pointMesh.Draw(null);
        }

    }
}
