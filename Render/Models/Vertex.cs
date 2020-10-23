using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render.Models
{
    struct Vertex
    {
        public vec3 Position;
        public vec3 Normal;
        public vec2 TexCoords;
        public vec3 Tangent;
        public vec3 Bitangent;

        public static Vertex[] FromPosAndTexCoords(float[] data)
        {
            Vertex[] verts = new Vertex[data.Length / 4];

            for (int i = 0; i < verts.Length; i++)
            {
                Vertex v = new Vertex();
                int di = i * 4;
                v.Position = new vec3(data[di], data[di + 1], 0);
                v.TexCoords = new vec2(data[di + 2], data[di + 3]);
                verts[i] = v;
            }

            return verts;
        }
    }
}
