using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Core.Curve
{
    class LinearCurve : ICurve
    {
        public float GetValueAt(float index)
            => index;
    }
}
