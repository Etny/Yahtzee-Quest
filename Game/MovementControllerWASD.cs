using GlmSharp;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Game
{
    class MovementControllerWASD : IMovementController
    {
        public delegate vec3 DirectionFunction();

        private float speed;
        private DirectionFunction dirFunc;

        public MovementControllerWASD(float speed, DirectionFunction dirFunc)
        {
            this.speed = speed;
            this.dirFunc = dirFunc;
        }

        public void UpdateMovement(Time deltaTime, Entity e)
        {
            float camSpeed = (float)(speed * deltaTime.Delta);
            var input = Program.InputManager;

            if (input.IsPressed(Keys.W))
                e.Position += dirFunc() * camSpeed;
            if (input.IsPressed(Keys.S))
                e.Position -= dirFunc() * camSpeed;
            if (input.IsPressed(Keys.A))
                e.Position -= vec3.Cross(dirFunc(), vec3.UnitY).Normalized * camSpeed;
            if (input.IsPressed(Keys.D))
                e.Position += vec3.Cross(dirFunc(), vec3.UnitY).Normalized * camSpeed;
        }
    }
}
