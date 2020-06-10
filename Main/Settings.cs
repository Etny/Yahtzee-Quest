using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Main
{
    //This will eventually read from some kind of settings file
    class Settings
    {

        public System.Drawing.Size GetShadowMapSize()
            => new System.Drawing.Size(1028, 1028);

        public void GetShadowMapSize(out int width, out int height)
        {
            width = 1028;
            height = 1028;
        }

    }
}
