using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;


namespace Yahtzee.Render
{
    class Shader : GLObject
    {
        public Shader(string shaderPath) : this(shaderPath, null, shaderPath) { }
        public Shader(string vertexPath, string fragmentPath) : this(vertexPath, null, fragmentPath) { }

        public Shader(string vertexPath, string geometryPath, string fragmentPath) : base()
        {
            uint vert = 0, geom = 0, frag = 0;

            if (geometryPath != null && !File.Exists($"Resource/Shaders/{geometryPath}.geom"))
                geometryPath = null;

            LoadShader(ref vert, ShaderType.VertexShader, $"Resource/Shaders/{vertexPath}.vert");
            if(geometryPath != null) LoadShader(ref geom, ShaderType.GeometryShader, $"Resource/Shaders/{geometryPath}.geom");
            LoadShader(ref frag, ShaderType.FragmentShader, $"Resource/Shaders/{fragmentPath}.frag");

            ID = gl.CreateProgram();
            gl.AttachShader(ID, vert);
            if (geometryPath != null) gl.AttachShader(ID, geom);
            gl.AttachShader(ID, frag);
            gl.LinkProgram(ID);

            string log = gl.GetProgramInfoLog(ID);
            if (!string.IsNullOrWhiteSpace(log))
                Console.WriteLine($"Shader program link error: {log}");

            gl.DeleteShader(vert);
            if (geometryPath != null) gl.DeleteShader(geom);
            gl.DeleteShader(frag);

            if ((GLEnum)gl.GetUniformBlockIndex(ID, "Matrices") != GLEnum.InvalidIndex)
                gl.UniformBlockBinding(ID, gl.GetUniformBlockIndex(ID, "Matrices"), 0);
        }


        private void LoadShader(ref uint shader, ShaderType type, string path)
        {
            shader = gl.CreateShader(type);
            gl.ShaderSource(shader, File.ReadAllText(path));
            gl.CompileShader(shader);

            string log = gl.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(log))
                Console.WriteLine($"Shader compile error when compiling shader at {path} of type {type}: {log}");
        }

        public override void Use() 
            => gl.UseProgram(ID);

        public override void Dispose()
            => gl.DeleteProgram(ID);

        public void SetFloat(string uniform, float f)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), f);
        }

        public void SetBool(string uniform, bool b)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), b ? 1 : 0);
        }

        public void SetInt(string uniform, int i)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), i);
        }

        public void SetVec2(string uniform, vec2 vec)
        {
            Use();
            gl.Uniform2(gl.GetUniformLocation(ID, uniform), vec.x, vec.y);
        }

        public void SetVec3(string uniform, vec3 vec)
        {
            Use();
            gl.Uniform3(gl.GetUniformLocation(ID, uniform), vec.x, vec.y, vec.z);
        }

        public void SetVec3(string uniform, float x, float y, float z)
            => SetVec3(uniform, new vec3(x, y, z));

        public void SetVec4(string uniform, vec4 vec)
        {
            Use();
            gl.Uniform4(gl.GetUniformLocation(ID, uniform), vec.x, vec.y, vec.z, vec.w);
        }

        public unsafe void SetMat3(string uniform, mat3 mat3)
        {
            Use();
            gl.UniformMatrix3(gl.GetUniformLocation(ID, uniform), 1, false, (float*)&mat3);
        }

        public unsafe void SetMat4(string uniform, mat4 mat4)
        {
            Use();
            gl.UniformMatrix4(gl.GetUniformLocation(ID, uniform), 1, false, (float*)&mat4);
        }

    }
}
