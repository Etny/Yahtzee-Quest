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
        public CollisionResult result = null;
        private readonly PhysicsManager pm;

        private vec3[] pointColor1 = { new vec3(0.807f, 0.070f, 0.792f) };
        private vec3[] pointColor2 = { new vec3(0.3f, 0.8f, 0.4f) };
        List<LineMesh> points = new List<LineMesh>();

        public ContactPointVisualizer(PhysicsManager pm)
        {
            this.pm = pm;
        }

        public void SetResult(CollisionResult result)
            => this.result = result;

        public (LineMesh, LineMesh) AddPoints((vec3, vec3) p)
        {
            points.Add(new LineMesh(new vec3[] { p.Item1 }, pointColor1));
            points.Add(new LineMesh(new vec3[] { p.Item2 }, pointColor2));

            return (points[points.Count-2], points[points.Count-1]);
        }

        public void RemovePoints((LineMesh, LineMesh) remove)
        {
            points.Remove(remove.Item1);
            points.Remove(remove.Item2);
        }


        [Conditional("DEBUG")]
        public void UpdateContactPoints()
        {
            if (result == null) return;

            var info = pm.DepthDetector.GetPenetrationInfo(result);
            var p = pm.DepthDetector.GetContactInfo(info);

            if (p.Item1 == vec3.NaN) return;

            Console.WriteLine($"Contact Point(s): {p.Item1} | {p.Item2}");

            AddPoints(p);
        }
        public void Draw()
        {
            if (points.Count <= 0) return;

            foreach (var p in points) p.Draw(null);
        }

    }
}
