using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Main;

namespace Yahtzee.Core.Physics.Constraints
{
    interface IConstraint
    {

        public bool StillValid();

        public void Resolve(Time deltaTime, int iter);


    }
}
