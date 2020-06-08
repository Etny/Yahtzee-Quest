using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;

namespace Yahtzee.Render
{
    abstract class Light
    {
        public vec3 Ambient = new vec3(0.05f, 0.05f, 0.05f);
        public vec3 Diffuse = new vec3(0.8f, 0.8f, 0.8f);
        public vec3 Specular = new vec3(1.0f, 1.0f, 1.0f);

        public Light() { }

        public Light(vec3 ambient, vec3 diffuse, vec3 specular)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public abstract void SetValues(Shader shader, int index);
        protected virtual void SetColorValues(Shader shader, string name)
        {
            shader.SetVec3(name + ".color.ambient", Ambient);
            shader.SetVec3(name + ".color.diffuse", Diffuse);
            shader.SetVec3(name + ".color.specular", Specular);
        }
    }

    class DirectionalLight : Light
    {
        public vec3 Direction;

        public DirectionalLight(vec3 direction) : base()
        {
            Direction = direction;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "dirLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".direction", Direction);
        }
    }

    class PointLight : Light
    {
        public vec3 Position;
        public float Constant = 1, Linear = .09f, Quadratic = .032f;

        public PointLight(vec3 position) : base()
        {
            Position = position;
        }
        public PointLight(vec3 position, float constant, float linear, float quadratic) : base()
        {
            Position = position;

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "pointLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".position", Position);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }
    }

    class SpotLight : Light
    {
        public vec3 Position;
        public vec3 Direction = new vec3(0, 0, 1);
        public float Cutoff = .5f, OuterCutoff = .6f;
        public float Constant = 1, Linear = 1, Quadratic = 1;

        public SpotLight(vec3 position, float cutoff, float outerCutoff) : this(position, cutoff, outerCutoff, 1, 0.09f, 0.032f) { }

        public SpotLight(vec3 position, float cutoff, float outerCutoff, float constant, float linear, float quadratic) : base()
        {
            Position = position;

            Cutoff = (float)Math.Cos(cutoff);
            OuterCutoff = (float)Math.Cos(outerCutoff);

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "spotLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".position", Position);
            shader.SetVec3(name + ".direction", Direction);
            shader.SetFloat(name + ".cutoff", Cutoff);
            shader.SetFloat(name + ".outerCutoff", OuterCutoff);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }
    }
}
