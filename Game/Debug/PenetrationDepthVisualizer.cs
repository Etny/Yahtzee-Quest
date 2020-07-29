﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assimp;
using GlmSharp;
using Silk.NET.OpenGL;
using Yahtzee.Game.Debug;
using Yahtzee.Game.Physics;
using Yahtzee.Render;
using static Yahtzee.Game.Physics.PenetrationDepthDetector;

namespace Yahtzee.Game
{
    /// <summary>
    /// Displays the inner workings of the Penetration Depth function using meshes. Used for debugging.
    /// </summary>
    unsafe class PenetrationDepthVisualizer
    {
        private readonly CollisionResult result;
        private readonly PhysicsManager pm;

        private List<Triangle> tris = new List<Triangle>();
        private vec3 newPoint = vec3.NaN;
        private Triangle closest = null;

        private List<LineMesh> TriMeshes = new List<LineMesh>();
        private LineMesh pointMesh;
        private LineMesh originMesh;

        private vec3[] triColors = { new vec3(0.960f, 0.956f, 0.078f), new vec3(0.960f, 0.956f, 0.078f), new vec3(0.960f, 0.956f, 0.078f) };
        private vec3[] triColorsClosest = { new vec3(1, 0, 0), new vec3(1, 0, 0), new vec3(1, 0, 0) };
        private vec3[] pointColor = { new vec3(0.807f, 0.070f, 0.792f) };
        private vec3[] originColors = { new vec3(0.098f, 0.494f, 0.960f), new vec3(0.415f, 0.960f, 0.098f) };
        private vec3[] normalColors = { new vec3(0.945f, 0.964f, 0.035f), new vec3(0.964f, 0.035f, 0.117f) };

        private int counter = 0;

        private string[] messages = { "Setup", "Find Closest", "New Point", "Return or Subdivide" };

        public PenetrationDepthVisualizer(CollisionResult result, PhysicsManager pm)
        {
            this.result = result;
            this.pm = pm;

            pointMesh = new LineMesh(null, pointColor);
            originMesh = new LineMesh(new vec3[] { vec3.Zero, vec3.UnitY }, originColors);
        }

        [Conditional("DEBUG")]
        public void UpdateDepthTest()
        {
            Console.WriteLine(messages[counter]);

            vec3 r = pm.DepthDetector.GetPenetrationDepthStep(result, ref tris, ref closest, ref newPoint, ref counter);

            if (r != vec3.NaN)
            {
                Console.WriteLine("Returned: " + r);
                TriMeshes.Clear();
                counter = 0;
                newPoint = vec3.NaN;
                return;
            }

            TriMeshes.ForEach(t => t.Dispose());
            TriMeshes.Clear();
            tris.ForEach(t => TriMeshes.Add(new LineMesh(t.Points, t == closest ? triColorsClosest : triColors, t == closest ? 15 : 5)));
            tris.ForEach(t => TriMeshes.Add(new LineMesh(new vec3[] { t.Center, t.Center + t.Normal.Normalized }, normalColors, newPoint == vec3.NaN || vec3.Dot(t.Normal, newPoint) < 0 ? 5 : 15)));


            if (counter == 2) 
                newPoint = tris.Find(t => t == closest).ClosestPoint();

            if (newPoint != vec3.NaN) pointMesh.SetPoints(new vec3[] { newPoint });
        }
        public void Draw()
        {
            if (TriMeshes.Count <= 0) return;

            TriMeshes.ForEach(x => x.Draw(null));
            pointMesh.Draw(null);
            originMesh.Draw(null);
        }

    }
}