using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Main
{
    //This will eventually read from some kind of settings file
    class Settings
    {
        private static int ShadowWidth = 512, ShadowHeight = 512;
        private static float LightNear = .1f, LightFar = 50f;

        public static readonly vec2 BaseScreenSize = new vec2(1920, 1080);
        public static readonly vec2 CurrentScreenSize = new vec2(1280, 720);
        public static vec2 ScreenRatio { get { return CurrentScreenSize / BaseScreenSize; } }

        public readonly int MaxLights = 4;

        public System.Drawing.Size GetShadowMapSize()
            => new System.Drawing.Size(ShadowWidth, ShadowHeight);

        public void GetShadowMapSize(out int width, out int height)
        {
            width = ShadowWidth;
            height = ShadowHeight;
        }
        public void GetLightRange(out float near, out float far)
        {
            near = LightNear;
            far = LightFar;
        }

    }
}
