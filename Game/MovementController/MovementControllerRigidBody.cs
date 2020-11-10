using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Physics;
using Yahtzee.Game.Entities;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Physics
{
    class MovementControllerRigidBody : IMovementController
    {

        public RigidBody RigidBody;
        public CollisionMesh Collision { get { return RigidBody.Collision; } }

        public MovementControllerRigidBody(Entity e, bool register = true)
        {
            RigidBody = new RigidBody(e, "Basic/Cube.obj");

            if(register) Register();
        }

        public void UpdateMovement(Time deltaTime, Entity e)
        {
            RigidBody.Update(deltaTime);
        }

        public void Register()
            => Program.PhysicsManager.RegisterRigidBody(RigidBody);
        

        public void Deregister()
            => Program.PhysicsManager.DeregisterRigidBody(RigidBody);
        
    }
}
