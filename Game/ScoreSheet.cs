using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Curve;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render.Textures;
using Yahtzee.Render.UI;
using Yahtzee.Render.UI.RenderComponent;

namespace Yahtzee.Game
{
    class ScoreSheet
    {

        private UILayer _layer;
        public readonly Font Font;

        public ButtonComponent Sheet { get; private set; }

        private float smallSize = .5f, fullSize = 1.05f;
        private vec2 maxScale;
        private float scaleTime = .3f, scaleProgress = 0f;
        private ICurve scaleCurve = new BezierCurve(new vec2(1, 0), new vec2(.61f, .94f));

        private float screenPadding = 30;

        public ScoreSheet(UILayer layer, Font font)
        {
            _layer = layer;
            Font = font;
            maxScale = new vec2(fullSize / smallSize);

            var rc = new ImageRenderComponent(layer.Gl, "Resource/Images/UI/Other/scorePaper2.png");
            Sheet = new ButtonComponent(layer, smallSize * (vec2)(rc.Image.Size), rc);
            Sheet.Transform.Translation = new vec2((-layer.UIFrameBuffer.BoundTexture.Size.x / 2) + (Sheet.Quad.Size.x / 2) + screenPadding, 0);

            layer.AddComponent(Sheet);
        }

        public void Update(Time deltaTime)
        {
            if (Sheet.Hovered && scaleProgress < scaleTime)
            {
                scaleProgress += deltaTime.DeltaF;
                if (scaleProgress > scaleTime) scaleProgress = scaleTime;
            }
            else if (!Sheet.Hovered && scaleProgress > 0)
            {
                scaleProgress -= deltaTime.DeltaF;
                if (scaleProgress < 0) scaleProgress = 0;
            }
            else return;

            float ratio = scaleCurve[scaleProgress / scaleTime];

            Sheet.Transform.Scale = vec2.Lerp(vec2.Ones, maxScale, ratio);
            Sheet.Transform.Translation = new vec2((-_layer.UIFrameBuffer.BoundTexture.Size.x / 2) + ((Sheet.Transform.Scale * Sheet.Quad.Size).x / 2) + (screenPadding - (ratio * 15)), 0);
        }


    }
}
