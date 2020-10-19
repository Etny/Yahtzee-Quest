using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;
using Yahtzee.Main;

namespace Yahtzee.Game
{
    interface IMovementController
    {
        public void UpdateMovement(Time deltaTime, Entity e);
    }
}
