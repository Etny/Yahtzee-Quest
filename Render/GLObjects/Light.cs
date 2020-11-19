using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GlmSharp;
using Silk.NET.OpenGL;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render
{

    abstract class Light
    {
        public vec3 Ambient = new vec3(0.05f, 0.05f, 0.05f);
        public vec3 Diffuse = new vec3(0.8f, 0.8f, 0.8f);
        public vec3 Specular = new vec3(1.0f, 1.0f, 1.0f);

        public Texture ShadowMap { get; protected set; } = null;
        public mat4 LightSpace { get; protected set; } = mat4.AllNaN;
        public bool ShadowsEnabled { get; protected set; } = false;


        public Light() { }

        public Light(vec3 ambient, vec3 diffuse, vec3 specular)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public abstract void SetValues(Shader shader, int index, ref int shadowMapUnit);

        public virtual void SetShadowsEnabled(bool shadows)
        {
            ShadowsEnabled = shadows;

            if (!shadows)
            {
                ShadowMap?.Dispose();
                ShadowMap = null;
            }
            else
            {
                CreateShadowMap();
                CalculateLightSpace();
            }
        }

        public virtual void CalculateShadows(FrameBuffer fb, Shader depthShader, Action<Shader> render)
        {
            fb.BindTexture(ShadowMap, GLEnum.DepthAttachment);
            depthShader.SetMat4("lightSpace", LightSpace);
            Util.GLClear();
            render(depthShader);
        }

        protected virtual void CreateShadowMap()
        {
            Program.Settings.GetShadowMapSize(out int width, out int height);
            ShadowMap = new DepthTexture(width, height);
        }

        protected abstract void CalculateLightSpace();

        protected virtual void SetColorValues(Shader shader, string name)
        {
            shader.SetVec3(name + ".ambient", Ambient);
            shader.SetVec3(name + ".diffuse", Diffuse);
            shader.SetVec3(name + ".specular", Specular);
        }

        protected virtual void SetShadowValues(Shader shader, string name, ref int shadowMapUnit)
        {
            shader.SetBool(name + ".shadowsEnabled", ShadowsEnabled);

            if (!ShadowsEnabled) return;

            shader.SetMat4(name + ".lightSpace", LightSpace);
            shader.SetInt(name + ".shadowIndex", shadowMapUnit);
            shader.SetInt("shadowMaps[" + shadowMapUnit + "]", shadowMapUnit + 8);
            ShadowMap.BindToUnit(shadowMapUnit + 8);
            shadowMapUnit += 1;
        }
    }

    class DirectionalLight : Light
    {
        public vec3 Direction { 
            get { return _direction; }

            set 
            {
                _direction = value;
                if (ShadowsEnabled) CalculateLightSpace();
            } 
        }

        private vec3 _direction;

        public DirectionalLight(vec3 direction) : base()
        {
            Direction = direction.Normalized;
        }

        public override void SetValues(Shader shader, int index, ref int shadowMapUnit)
        {
            string name = "Lights[" + index + "]";

            base.SetColorValues(shader, name);
            base.SetShadowValues(shader, name, ref shadowMapUnit);

            shader.SetInt(name + ".type", 2);
            shader.SetVec3(name + ".direction", Direction);
        }

        protected override void CalculateLightSpace()
        {
            Program.Settings.GetLightRange(out float near, out float far);

            Transform t = Transform.Identity;
            t.Orientation = quat.FromAxisAngle((float)Math.Acos(vec3.Dot(Direction.NormalizedSafe, vec3.UnitZ)), vec3.Cross(Direction, vec3.UnitZ).NormalizedSafe);

            mat4 Projection = mat4.Ortho(-20, 20, -20, 20, near, far);
            mat4 LookAt = mat4.LookAt(Direction * -10, vec3.Zero, t * vec3.UnitY);
            LightSpace = Projection * LookAt;
        }
    }

    class PointLight : Light
    {
        public vec3 Position
        {
            get { return _position; }

            set
            {
                _position = value;
                if (ShadowsEnabled) CalculateLightSpace();
            }
        }

        private vec3 _position;

        public float Constant = 1, Linear = .09f, Quadratic = .032f;

        private mat4[] CubeLookAts = null;

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

        protected override void SetShadowValues(Shader shader, string name, ref int shadowMapUnit)
        {
            shader.SetBool(name + ".shadowsEnabled", ShadowsEnabled);

            if (!ShadowsEnabled) return;

            shader.SetMat4(name + ".lightSpace", LightSpace);
            shader.SetInt("shadowCube", shadowMapUnit + 8);
            ShadowMap.BindToUnit(shadowMapUnit + 8);
            shadowMapUnit += 1;
        }

        public override void SetValues(Shader shader, int index, ref int shadowMapUnit)
        {
            string name = "Lights[" + index + "]";

            base.SetColorValues(shader, name);
            SetShadowValues(shader, name, ref shadowMapUnit);

            //if (!ShadowsEnabled) shader.SetInt(name + ".shadowMap", 30);
            shader.SetBool(name + ".shadowsEnabled", ShadowsEnabled);

            shader.SetInt(name + ".type", 0);
            shader.SetVec3(name + ".position", Position);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }

        protected override void CreateShadowMap()
        {
            Program.Settings.GetShadowMapSize(out int width, out int height);
            ShadowMap = new CubeMap(width, height, InternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.Float);
        }

        public override void CalculateShadows(FrameBuffer fb, Shader shader, Action<Shader> render)
        {
            shader.SetVec3("lightPos", Position);

            for (int i = 0; i < 6; i++)
            {
                fb.BindTexture(ShadowMap, GLEnum.DepthAttachment, GLEnum.TextureCubeMapPositiveX + i);
                Util.GLClear();
                shader.SetMat4("lightSpace", LightSpace * CubeLookAts[i]);
                render(shader);
            }
        }

        protected override void CalculateLightSpace()
        {
            if(CubeLookAts == null) CubeLookAts = new mat4[6];
            CubeLookAts[0] = mat4.LookAt(Position, Position + new vec3(1, 0, 0), new vec3(0, -1, 0));
            CubeLookAts[1] = mat4.LookAt(Position, Position + new vec3(-1, 0, 0), new vec3(0, -1, 0));
            CubeLookAts[2] = mat4.LookAt(Position, Position + new vec3(0, 1, 0), new vec3(0, 0, 1));
            CubeLookAts[3] = mat4.LookAt(Position, Position + new vec3(0, -1, 0), new vec3(0, 0, -1));
            CubeLookAts[4] = mat4.LookAt(Position, Position + new vec3(0, 0, 1), new vec3(0, -1, 0));
            CubeLookAts[5] = mat4.LookAt(Position, Position + new vec3(0, 0, -1), new vec3(0, -1, 0));

            Program.Settings.GetShadowMapSize(out int width, out int height);
            Program.Settings.GetLightRange(out float near, out float far);

            LightSpace = mat4.Perspective(Util.ToRad(90), width / height, near, far);
        }
    }

    class SpotLight : Light
    {
        public vec3 Position
        {
            get { return _position; }

            set
            {
                _position = value;
                if (ShadowsEnabled) CalculateLightSpace();
            }
        }
        public vec3 Direction
        {
            get { return _direction; }

            set
            {
                _direction = value;
                if (ShadowsEnabled) CalculateLightSpace();
            }
        }

        private vec3 _position;
        private vec3 _direction = new vec3(0, 0, 1);

        public float Cutoff = .5f, OuterCutoff = .6f;
        public float Constant = 1, Linear = 1, Quadratic = 1;

        public SpotLight(vec3 position, float cutoff, float outerCutoff) : this(position, cutoff, outerCutoff, 1, 0.09f, 0.032f) { }

        public SpotLight(vec3 position, float cutoff, float outerCutoff, float constant, float linear, float quadratic) : base()
        {
            _position = position;

            Cutoff = (float)Math.Cos(cutoff);
            OuterCutoff = (float)Math.Cos(outerCutoff);

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public override void SetValues(Shader shader, int index, ref int shadowMapUnit)
        {
            string name = "Lights[" + index + "]";

            base.SetColorValues(shader, name);
            base.SetShadowValues(shader, name, ref shadowMapUnit);

            shader.SetInt(name + ".type", 1);
            shader.SetVec3(name + ".position", Position);
            shader.SetVec3(name + ".direction", Direction);
            shader.SetFloat(name + ".cutoff", Cutoff);
            shader.SetFloat(name + ".outerCutoff", OuterCutoff);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }

        public void SetPositionAndDirection(vec3 position, vec3 direction)
        {
            _position = position;
            _direction = direction;
            if(ShadowsEnabled) CalculateLightSpace();
        }

        protected override void CalculateLightSpace()
        {
            Program.Settings.GetShadowMapSize(out int width, out int height);
            Program.Settings.GetLightRange(out float near, out float far);

            Transform t = Transform.Identity;
            t.Orientation = quat.FromAxisAngle((float)Math.Acos(vec3.Dot(Direction.NormalizedSafe, vec3.UnitZ)), vec3.Cross(Direction, vec3.UnitZ).NormalizedSafe);

            mat4 Projection = mat4.PerspectiveFov((float)(2 * Math.Acos(OuterCutoff)), width, height, near, far);
            mat4 LookAt = mat4.LookAt(Position, Position + Direction, t * vec3.UnitY);
            LightSpace = Projection * LookAt;
        }
    }
}
