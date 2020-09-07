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

        private vec3[] pointColor1 = { new vec3(0.807f, 0.070f, 0.792f) };
        private vec3[] pointColor2 = { new vec3(0.3f, 0.8f, 0.4f) };
        LineMesh pointMesh1, pointMesh2;
        (vec3, vec3) points = (vec3.NaN, vec3.NaN);

        public ContactPointVisualizer(CollisionResult result, PhysicsManager pm)
        {
            this.result = result;
            this.pm = pm;

            pointMesh1 = new LineMesh(null, pointColor1);
            pointMesh2 = new LineMesh(null, pointColor2);

        }

        [Conditional("DEBUG")]
        public void UpdateContactPoints()
        {
            var info = pm.DepthDetector.GetPenetrationInfo(result);
            var p = pm.DepthDetector.GetContactInfo(info);

            if (p.Item1 == vec3.NaN) return;

            points = p;

            Console.WriteLine($"Contact Point(s): {points}, M1 pos: {result.M1.Transform.Translation}");

            pointMesh1.SetPoints(new vec3[] { points.Item1 });
            //pointMesh2.SetPoints(new vec3[] { points.Item1 - (info.Item1.Normal * info.Item1.DistToOrigin()) });
            pointMesh2.SetPoints(new vec3[] { points.Item2 });
        }
        public void Draw()
        {
            if (points.Item1 == vec3.NaN) return;

            pointMesh1.Draw(null);
            pointMesh2.Draw(null);
        }

    }
}
