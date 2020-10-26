using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render
{
    interface IRenderable
    {
        FrameBuffer Render(FrameBuffer target);
    }
}
