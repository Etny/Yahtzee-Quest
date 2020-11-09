using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlmSharp;
using Yahtzee.Core.Physics;
using Yahtzee.Render.Models;

namespace Yahtzee.Core
{
    struct BoundingBox
    {
        public vec3 Min, Max;

        public BoundingBox(vec3 min, vec3 max)
        {
            Min = min;
            Max = max;
        }

        public static BoundingBox FromCorners(vec3 cA, vec3 cB)
        {
            var Min = new vec3(cA.x - cB.x <= 0 ? cA.x : cB.x,
                               cA.y - cB.y <= 0 ? cA.y : cB.y,
                               cA.z - cB.z <= 0 ? cA.z : cB.z);

            var Max = new vec3(cA.x - cB.x <= 0 ? cB.x : cA.x,
                               cA.y - cB.y <= 0 ? cB.y : cA.y,
                               cA.z - cB.z <= 0 ? cB.z : cA.z);

            return new BoundingBox(Min, Max);
        }

        public static BoundingBox FromMesh(ModelMesh m, Transform t, float padding = 0)
            => FromVertexPositions(m.Vertices.Select(v => v.Position).Distinct().ToArray(), t, padding);

        public static BoundingBox FromMesh(ModelMesh m, float padding = 0)
            => FromVertexPositions(m.Vertices.Select(v => v.Position).Distinct().ToArray(), padding);

        public static BoundingBox FromCollisionMesh(CollisionMesh m, Transform t, float padding = 0)
            => FromVertexPositions(m.CollisionVertices, t, padding);

        public static BoundingBox FromCollisionMesh(CollisionMesh m, float padding = 0)
            => FromVertexPositions(m.CollisionVertices, padding);

        public static BoundingBox FromVertexPositions(vec3[] vs, float padding = 0)
            => FromVertexPositions(vs, Transform.Identity, padding);

        public static BoundingBox FromVertexPositions(vec3[] vs, Transform t, float padding = 0)
        {
            vec3[] dirs = { vec3.UnitX, vec3.UnitY, vec3.UnitZ,
                           -vec3.UnitX, -vec3.UnitY, -vec3.UnitZ};

            float[] dots = new float[6];
            vec3[] verts = new vec3[6];

            for (int j = 0; j < vs.Length; j++)
            {
                var v = t * vs[j];

                for (int i = 0; i < 6; i++)
                {
                    var dir = dirs[i];
                    float d = vec3.Dot(v, dir);
                    if (j != 0 && dots[i] >= d) continue;
                    dots[i] = d;
                    verts[i] = v + padding * dir;
                }
            }

            return FromCorners(new vec3(verts[0].x, verts[1].y, verts[2].z), new vec3(verts[3].x, verts[4].y, verts[5].z));
        }

        public BoundingBox Translated(vec3 translation)
        {
            return new BoundingBox(Min + translation, Max + translation);
        }

        public bool Intersects(BoundingBox box)
            => Min.x <= box.Max.x && box.Min.x <= Max.x &&
                Min.y <= box.Max.y && box.Min.y <= Max.y &&
                Min.z <= box.Max.z && box.Min.z <= Max.z;

        public vec3[] GetPoints()
        {
            return new vec3[] {Min, new vec3(Max.x, Min.y, Min.z), new vec3(Min.x, Max.y, Min.z), new vec3(Min.x, Min.y, Max.z),
                               new vec3(Min.x, Max.y, Max.z), new vec3(Max.x, Min.y, Max.z), new vec3(Max.x, Max.y, Min.z), Max};

        }

        public Transform GetTransform()
        {
            Transform t = Transform.Identity;
            t.Translation = (Min + Max) / 2;
            t.Scale = Max - Min;
            return t;
        }
    }
}
