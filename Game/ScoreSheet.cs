using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core.Curve;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;
using Yahtzee.Render.UI;
using Yahtzee.Render.UI.RenderComponent;
using Yahtzee.Main;
using Yahtzee.Core;

namespace Yahtzee.Game
{
    class ScoreSheet: ButtonComponent
    {

        public readonly Font Font;

        private float smallSize = .5f, fullSize = 1.05f;
        private vec2 maxScale;
        private float scaleTime = .3f, scaleProgress = 0f;
        private ICurve scaleCurve = new BezierCurve(new vec2(1, 0), new vec2(.61f, .94f));

        private float screenPadding = 30;
        private float fullSizePadding = 15;
        private float smallSizeHeight = 130;
        private float fullSizeHeight = 60;

        public ScoreSheet(UILayer layer, Font font) : base(layer)
        {
            Font = font;
            maxScale = new vec2(fullSize / smallSize);

            var rc = new ImageRenderComponent(layer.Gl, "Resource/Images/UI/Other/scorePaper2.png");
            RenderComponent = rc;
            Quad = new QuadMesh((smallSize * (vec2)(rc.Image.Size)).ScaleToScreen());
            Transform.Translation = new vec2((-layer.UIFrameBuffer.BoundTexture.Size.x / 2) + (Quad.Size.x / 2) + screenPadding, smallSizeHeight);
        }

        public override void Update(Time deltaTime)
        {
            if (Hovered && scaleProgress < scaleTime)
            {
                scaleProgress += deltaTime.DeltaF;
                if (scaleProgress > scaleTime) scaleProgress = scaleTime;
            }
            else if (!Hovered && scaleProgress > 0)
            {
                scaleProgress -= deltaTime.DeltaF;
                if (scaleProgress < 0) scaleProgress = 0;
            }
            else return;

            float ratio = scaleCurve[scaleProgress / scaleTime];

            Transform.Scale = vec2.Lerp(vec2.Ones, maxScale, ratio);
            Transform.Translation = new vec2((-Layer.UIFrameBuffer.BoundTexture.Size.x / 2) + ((Transform.Scale * Quad.Size).x / 2) + (screenPadding - (ratio * (screenPadding - fullSizePadding))),
                                                   smallSizeHeight + ((fullSizeHeight - smallSizeHeight) * ratio));
        }


    }
}
