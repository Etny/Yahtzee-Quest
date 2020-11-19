using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Entities
{
    class EntityProxWall : ModelEntity
    {

        private IEnumerable<Entity> _entities;
        private Shader _shader;
        private GL _gl;

        public vec3 Color = new vec3(1);
        public float Threshold = 2f;

        public EntityProxWall(GL gl, string modelPath, IEnumerable<Entity> entities) : base(modelPath)
        {
            DrawInstanced = false;
            _gl = gl;
            _entities = entities;
            _shader = ShaderRepository.GetShader("Default/default", "Other/prox");
        }

        public override void Draw(Shader unused)
        {
            _shader.SetFloat("Threshold", Threshold);
            _shader.SetVec3("Color", Color);

            int i = 0;

            foreach (var e in _entities)
            {
                _shader.SetVec3("Points[" + i + "]", e.Transform.Translation);
                i++;
            }
            _shader.SetInt("PointCount", i);

            // _gl.Disable(EnableCap.CullFace);
            _gl.Enable(EnableCap.Blend);
            base.Draw(_shader);
            _gl.Disable(EnableCap.Blend);
            //_gl.Enable(EnableCap.CullFace);

        }

    }
}
