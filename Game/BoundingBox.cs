using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;

namespace Yahtzee.Game
{
    struct BoundingBox
    {
        public vec3 Min, Max;

        public BoundingBox(vec3 cA, vec3 cB)
        {
            Min = new vec3(cA.x - cB.x <= 0 ? cA.x : cB.x,
                           cA.y - cB.y <= 0 ? cA.y : cB.y,
                           cA.z - cB.z <= 0 ? cA.z : cB.z);

            Max = new vec3(cA.x - cB.x <= 0 ? cB.x : cA.x,
                           cA.y - cB.y <= 0 ? cB.y : cA.y,
                           cA.z - cB.z <= 0 ? cB.z : cA.z);
        }

        public void Update(vec3 cA, vec3 cB)
        {
            Min = new vec3(cA.x - cB.x <= 0 ? cA.x : cB.x,
                           cA.y - cB.y <= 0 ? cA.y : cB.y,
                           cA.z - cB.z <= 0 ? cA.z : cB.z);

            Max = new vec3(cA.x - cB.x <= 0 ? cB.x : cA.x,
                           cA.y - cB.y <= 0 ? cB.y : cA.y,
                           cA.z - cB.z <= 0 ? cB.z : cA.z);
        }

        public bool Intersects(BoundingBox box)
            =>  (Min.x <= box.Max.x && box.Min.x <= Max.x) &&
                (Min.y <= box.Max.y && box.Min.y <= Max.y) &&
                (Min.z <= box.Max.z && box.Min.z <= Max.z);
        
        public vec3[] GetPoints()
        {
            return new vec3[] {Min, new vec3(Max.x, Min.y, Min.z), new vec3(Min.x, Max.y, Min.z), new vec3(Min.x, Min.y, Max.z),
                               new vec3(Min.x, Max.y, Max.z), new vec3(Max.x, Min.y, Max.z), new vec3(Max.x, Max.y, Min.z), Max};

        }

        public Transform GetTransform()
        {
            Transform t = Transform.Identity;
            t.Translation = (Min + Max) / 2;
            t.Scale = (Max - Min);
            return t;
        }
    }
}
