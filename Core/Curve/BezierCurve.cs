using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Core.Curve
{
    class BezierCurve : ICurve
    {

        public vec2[] Points { get; protected set; }

        public BezierCurve(params vec2[] p)
        {
            Points = new vec2[p.Length + 2];
            Points[0] = vec2.Zero;
            Points[^1] = vec2.Ones;

            for (int i = 0; i < p.Length; i++)
                Points[i + 1] = p[i];
        }

        public float GetValueAt(float x)
        {
            if (x < 0)
                return 0;

            if (x == 0)
                return Points[0].y;

            int currentSize = Points.Length - 1;

            vec2[] points = Points;
            vec2[] interPoints = new vec2[currentSize];

            while (currentSize >= 1)
            {
                for (int i = 0; i < currentSize; i++)
                    interPoints[i] = Intermediate(points[i], points[i + 1], x);

                currentSize--;

                points = interPoints;
                interPoints = new vec2[currentSize];
            }

            return points[0].y;
        }

        //TODO: delet this
        private vec2 Intermediate(vec2 p1, vec2 p2, float f)
        {
            float x = p1.x + (f * (p2.x - p1.y));
            float y = p1.y + (f * (p2.y - p1.y));

            return new vec2(x, y);
        }
    }
}
