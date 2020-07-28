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

namespace Yahtzee.Game
{
    class PhysicsManager
    {
        public CollisionDetector Collisions;
        public PenetrationDepthDetector DepthDetector;

        public PhysicsManager()
        {
            Collisions = new CollisionDetector();
            DepthDetector = new PenetrationDepthDetector(this);

            vec3[] points = new vec3[] { new vec3(5, 4, -1), new vec3(10, -2, -1) };

            Console.WriteLine(points.GetClosetToOriginOnAffineHull());
        }

        public vec3 ClosestPoint(vec3 A, vec3 B, vec3 C)
        {
            mat3 orthoMatrix = new mat3();
            vec3[] points = { A, B, C };
            
            for(int i = 0; i < 9; i++)
            {
                if (i%3 == 0) { orthoMatrix[i] = 1; continue; }
                orthoMatrix[i] = vec3.Dot(points[i/3], (points[i%3] - A));
            }

            var result = (orthoMatrix.Inverse * mat3.Identity).Column0;

            return (A * result.x) + (B * result.y) + (C * result.z);
        }

        private vec3 ClosestToOrigin(vec3 A, vec3 B)
        {
            mat2 orthoMatrix = new mat2(1, vec3.Dot(A, (B - A)), 1, vec3.Dot(B, (B - A)));

            var result = orthoMatrix.Inverse.Column0;

            if (result.y < 0) return A;
            if (result.x < 0) return B;

            return (A * result.x) + (B * result.y);
        }

    }
}
