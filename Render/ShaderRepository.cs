using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render
{
    class ShaderRepository
    {
        private static readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        public static Shader GetShader(string path)
        {
            if (_shaders.ContainsKey(path))
                return _shaders[path];

            Shader shader = new Shader(path);
            _shaders.Add(path, shader);
            return shader;
        }

        public static Shader GetShader(string pathVert, string pathFrag)
        {
            if (_shaders.ContainsKey(pathVert+pathFrag))
                return _shaders[pathVert+pathFrag];

            Shader shader = new Shader(pathVert, pathFrag);
            _shaders.Add(pathVert+pathFrag, shader);
            return shader;
        }


    }
}
