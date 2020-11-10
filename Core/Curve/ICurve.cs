using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Core.Curve
{
    interface ICurve
    {
        float GetValueAt(float index);
        float this[float i] => GetValueAt(i);
    }
}
