using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render
{
    class ModelLoader
    {
        private static Dictionary<string, Model> ModelCache = new Dictionary<string, Model>();

        public static Model LoadModel(string path, bool collision = false)
        {
            if (!ModelCache.ContainsKey(path))
            {

            }

            return new Model(path, collision);
        }
    }
}
