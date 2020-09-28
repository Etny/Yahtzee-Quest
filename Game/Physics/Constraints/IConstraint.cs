using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Main;

namespace Yahtzee.Game.Physics.Constraints
{
    interface IConstraint
    {

        public bool StillValid();

        public void Resolve(Time deltaTime, int iter);

    
    }
}
