using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.UI
{
    class ButtonComponent : QuadComponent
    {

        public Texture Image { get; protected set; }

        private bool hovered = false;

        public ButtonComponent(UILayer layer, string imgPath) : base(layer)
        {
            Image = new ImageTexture(layer.Gl, imgPath, TextureType.Other);

            Quad = new QuadMesh(((vec2)Image.Size).ScaleToScreen());

            //_shader = ShaderRepository.GetShader("UI/UI", "UI/UIBlurImage");
            //_shader.SetInt("screen", 0);

            Transform.Translation -= new vec2(0, Util.BaseScreenSize.y/4).ScaleToScreen();
        }

        public override void Draw()
        {
            //Image.BindToUnit(1);
            //_shader.SetInt("image", 1);
            _shader.SetVec3("color", hovered ? new vec3(1, 0, 0) : new vec3(0, 0, 1));
            base.Draw();
        }

        public override void Update(Time deltaTime)
        {
            Transform.Orientation += (float)Math.Sin(deltaTime.DeltaF);

            vec2 mPos = Program.InputManager.MousePosition.ToUISpace();

            if (Transform.Orientation % Math.PI > float.Epsilon) 
                mPos = Transform.Translation + (quat.FromAxisAngle(-Transform.Orientation, vec3.UnitZ) * new vec3(mPos - Transform.Translation, 0)).xy;

            var size = Quad.Size;

            var min = Transform.Translation - (size / 2);
            var max = Transform.Translation + (size / 2);

            hovered = mPos.x >= min.x && mPos.x <= max.x && mPos.y >= min.y && mPos.y <= max.y;
        }
    }
}
