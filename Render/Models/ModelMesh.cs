using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using GlmSharp;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.Models
{
    class ModelMesh : Mesh<Vertex>
    {

        public ImageTexture[] Textures;


        public ModelMesh(Vertex[] vertices, uint[] indices, ImageTexture[] textures) : base(vertices, indices)
        {
            Textures = textures;
        }

        public ModelMesh(Vertex[] vertices) : this(vertices, null, new ImageTexture[0]) { }

        protected unsafe override void SetupVertexAttributePointers()
        {
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(8 * sizeof(float)));
            gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(11 * sizeof(float)));
            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);
            gl.EnableVertexAttribArray(2);
            gl.EnableVertexAttribArray(3);
            gl.EnableVertexAttribArray(4);
        }

        public virtual void AddTexture(ImageTexture texture)
        {
            Array.Resize(ref Textures, Textures.Length + 1);
            Textures[^1] = texture;
        }

        public override unsafe void Draw(Shader shader, int count = 1)
        { 
            shader.Use();

            if (!shader.LightingShader)
            {
                int diffuseNumber = 1;
                int specularNumber = 1;
                int normalNumber = 1;

                for (int i = 0; i < Textures.Length; i++)
                {
                    string textureName = "unknown";

                    switch (Textures[i].TextureType)
                    {
                        case TextureType.Diffuse:
                            textureName = "texture_diffuse" + diffuseNumber++;
                            break;
                        case TextureType.Specular:
                            textureName = "texture_specular" + specularNumber++;
                            break;
                        case TextureType.Normal:
                            textureName = "texture_normal" + normalNumber++;
                            break;
                    }

                    shader.SetInt($"material.{textureName}", i);

                    Textures[i].BindToUnit(i);
                }

                shader.SetBool("material.usingDiffuseMap", diffuseNumber > 1);
                shader.SetBool("material.usingSpecularMap", specularNumber > 1);
                shader.SetBool("material.usingNormalMap", normalNumber > 1);

                if (diffuseNumber == 1) shader.SetVec3("material.diffuseColor", new vec3(0.5f));
                if (specularNumber == 1) shader.SetFloat("material.specularComponent", 0.5f);
            }

            gl.ActiveTexture(TextureUnit.Texture0);

            gl.BindVertexArray(VAO);
            if (Indices != null) gl.DrawElementsInstanced((GLEnum)PrimitiveType.Triangles, (uint)Indices.Length, (GLEnum)DrawElementsType.UnsignedInt, (void*)0, (uint)count);
            else gl.DrawArraysInstanced((GLEnum)PrimitiveType.Triangles, 0, (uint)Vertices.Length, (uint)count);
        }
    }
}
