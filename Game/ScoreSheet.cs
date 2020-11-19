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
using Yahtzee.Game.SheetFields;
using sff = Yahtzee.Game.SheetFields.SheetFieldFunctions;
using System.Linq;

namespace Yahtzee.Game
{
    class ScoreSheet : ButtonComponent
    {

        public readonly Font Font;

        public readonly float smallSize = .5f, fullSize = 1.05f;
        public readonly vec2 maxScale;
        private float scaleTime = .3f, scaleProgress = 0f;
        private ICurve scaleCurve = new BezierCurve(new vec2(1, 0), new vec2(.61f, .94f));

        private float screenPadding = 30;
        private float fullSizePadding = 15;
        private float smallSizeHeight = 130;
        private float fullSizeHeight = 60;
        private vec2 imgSize;

        private static List<SheetField> fields = new List<SheetField>();
        private float[] fieldLocs = {218, 132,
                                     218, 194,
                                     218, 263,
                                     218, 327,
                                     218, 392,
                                     218, 456,
                                     218, 520,
                                     218, 584,
                                     218, 645,
                                     572, 132,
                                     572, 194,
                                     572, 263,
                                     572, 327,
                                     572, 392,
                                     572, 456,
                                     572, 520,
                                     572, 645,
                                     494, 702};
        private Func<int[], int>[] fieldFuncs = {sff.NumFunc(1),
                                                 sff.NumFunc(2),
                                                 sff.NumFunc(3),
                                                 sff.NumFunc(4),
                                                 sff.NumFunc(5),
                                                 sff.NumFunc(6),
                                                 sff.TotalOfRange(fields, true, 0, 6),
                                                 sff.Threshold(sff.TotalOfRange(fields, true, 6, 1), 2, sff.FixedValue(35)),
                                                 sff.TotalOfRange(fields, false, 6, 2),
                                                 sff.OfAKind(3, sff.Sum),
                                                 sff.OfAKind(4, sff.Sum),
                                                 sff.FullHouse,
                                                 sff.Straight(3, sff.FixedValue(30)),
                                                 sff.Straight(4, sff.FixedValue(40)),
                                                 sff.Sum,
                                                 sff.OfAKind(5, sff.FixedValue(50)),
                                                 sff.TotalOfRange(fields, true, 9, 6),
                                                 sff.TotalOfFields(fields, false, 8, 16)};
        private int[] nonLockableIndices = new int[] { 6, 7, 8, 16, 17};

        public Func<bool> CanScore = () => true;
        public event Action OnSelect;

        public ScoreSheet(UILayer layer, Font font) : base(layer)
        {
            Font = font;
            maxScale = new vec2(fullSize / smallSize);

            var rc = new ImageRenderComponent(layer.Gl, "Resource/Images/UI/Other/scorePaper2.png");
            RenderComponent = rc;
            imgSize = ((vec2)rc.Image.Size * smallSize).ScaleToScreen() ;
            Quad = new QuadMesh(imgSize);
            Transform.Translation = new vec2((-layer.UIFrameBuffer.BoundTexture.Size.x / 2) + (Quad.Size.x / 2) + screenPadding, smallSizeHeight);

            layer.AddComponent(this);

            Font.Size = 30;
            
            for (int i = 0; i < 18; i++)
            {
                var t = new SheetField(this, layer, Font, fieldFuncs[i], !nonLockableIndices.Contains(i), new vec2(fieldLocs[i*2], -fieldLocs[i*2 +1]));
                fields.Add(t);
            }

            OnClick += SheetClicked;
            ClearFields();
        }

        private void SheetClicked(object sender, EventArgs e)
        {
            if (Transform.Scale != maxScale || !CanScore()) return;

            var mp = Program.InputManager.MousePosition.ToUISpace();

            var closest = fields[0];
            float dist = (closest.TextField.Transform.Translation - mp).Length;

            foreach (var f in fields)
            {
                if ((f.TextField.Transform.Translation - mp).Length >= dist) continue;

                dist = (f.TextField.Transform.Translation - mp).Length;
                closest = f;
            }

            OnSelect?.Invoke();
            closest.Lock();
            ClearFields();
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
            var newScale = vec2.Lerp(vec2.Ones, maxScale, ratio);

            Transform.Scale = newScale;
            Transform.Translation = new vec2((-Layer.UIFrameBuffer.BoundTexture.Size.x / 2) + ((Transform.Scale * Quad.Size).x / 2) + (screenPadding - (ratio * (screenPadding - fullSizePadding))),
                                                   smallSizeHeight + ((fullSizeHeight - smallSizeHeight) * ratio));

            fields.ForEach(f => f.Update());
        }

        public void ClearFields()
        {
            fields.ForEach(f => { if (f.Lockable) f.Clear(); else f.UpdateText(null); });
        }

        public void UpdateRolled(int[] rolled)
        {
           fields.ForEach(f => f.UpdateText(rolled));
        }
    }
}
