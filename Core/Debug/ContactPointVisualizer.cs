using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assimp;
using GlmSharp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.Formats.Jpeg;
using Yahtzee.Game;
using Yahtzee.Game.Physics;
using Yahtzee.Render;
using static Yahtzee.Game.Physics.PenetrationDepthDetector;

namespace Yahtzee.Core.Debug
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
        Dictionary<int, LineMesh> points = new Dictionary<int, LineMesh>();
        private static int ID = 0;

        public ContactPointVisualizer(PhysicsManager pm)
        {
            this.pm = pm;
        }

        public void SetResult(CollisionResult result)
            => this.result = result;

        public (LineMesh, LineMesh, int, int) AddPoints((vec3, vec3) p)
        {
            var l1 = new LineMesh(new vec3[] { p.Item1 }, pointColor1);
            var l2 = new LineMesh(new vec3[] { p.Item2 }, pointColor2);
            points.Add(ID++, l1);
            //points.Add(ID++, l2);
            ID++;
            return (l1, l2, ID - 2, ID - 1);
        }

        public void RemovePoints((LineMesh, LineMesh, int, int) remove)
        {
            points.Remove(remove.Item3);
            points.Remove(remove.Item4);
        }


        [Conditional("DEBUG")]
        public void UpdateContactPoints()
        {
            if (result == null) return;
            if (!result.Colliding) return;

            var info = pm.DepthDetector.GetPenetrationInfo(result);
            var p = pm.DepthDetector.GetContactInfo(info);

            if (p.Item1 == vec3.NaN) return;

            Console.WriteLine($"Contact Point(s): {p.Item1} | {p.Item2}");

            AddPoints(p);
        }
        public void Draw()
        {
            if (points.Count <= 0) return;

            foreach (var p in points.Values) p.Draw(null);
        }

    }
}
