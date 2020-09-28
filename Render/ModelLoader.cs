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
            string key = path + (collision ? "col" : "");

            if (!ModelCache.ContainsKey(key))
                ModelCache.Add(key, new Model(path, collision));

            return ModelCache.GetValueOrDefault(key);
        }
    }
}
