using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Main
{
    //This will eventually read from some kind of settings file
    class Settings
    {
        private static int ShadowWidth = 1024, ShadowHeight = 1024;
        private static float LightNear = .1f, LightFar = 1000f;

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
