﻿using System;
using System.Collections.Generic;
using System.Text;
using ai = Assimp;
using System.Numerics;
using Silk.NET.OpenGL;
using GlmSharp;
using Yahtzee.Render.Textures;
using Yahtzee.Game;
using Yahtzee.Core.Physics;
using Yahtzee.Core;

namespace Yahtzee.Render.Models
{
    class Model
    {

        private GL gl;

        public List<Mesh<Vertex>> Meshes;
        private List<ImageTexture> loadedTextures;

        private string directory;
        public readonly string Key;

        private delegate Mesh<Vertex> MeshLoader(ai.Mesh mesh, ai.Scene scene);

        public Model(string filePath, bool collision = false)
        {
            gl = GL.GetApi();

            Key = filePath + (collision ? "col" : "");

            if (!collision) loadModel("Resource/Models/" + filePath, proccessMesh);
            else loadCollisionModel("Resource/Models/" + filePath, proccessCollisionMesh);
        }

        public static CollisionMesh LoadCollisionMesh(string filePath)
        {
            var mesh = (CollisionMesh)new Model(filePath, true).Meshes[0];
            return mesh;
        }


        private void loadModel(string path, MeshLoader loader)
        {
            var assimp = new ai.AssimpContext();

            ai.Scene scene = assimp.ImportFile(Util.AbsolutePath(path), ai.PostProcessSteps.Triangulate | ai.PostProcessSteps.CalculateTangentSpace);

            if (scene == null || scene.RootNode == null || scene.SceneFlags.HasFlag(ai.SceneFlags.Incomplete))
            {
                Console.WriteLine($"Assimp error when importing file \'{path}\'");
                return;
            }

            directory = path.Substring(0, path.LastIndexOf("/") + 1);
            loadedTextures = new List<ImageTexture>();
            Meshes = new List<Mesh<Vertex>>();

            proccessNode(scene.RootNode, scene, loader);
        }

        private void loadCollisionModel(string path, MeshLoader loader)
        {
            var assimp = new ai.AssimpContext();

            ai.Scene scene = assimp.ImportFile(Util.AbsolutePath(path), ai.PostProcessSteps.DropNormals);

            if (scene == null || scene.RootNode == null || scene.SceneFlags.HasFlag(ai.SceneFlags.Incomplete))
            {
                Console.WriteLine($"Assimp error when importing file \'{path}\'");
                return;
            }

            directory = path.Substring(0, path.LastIndexOf("/") + 1);
            Meshes = new List<Mesh<Vertex>>();

            proccessNode(scene.RootNode, scene, loader);
        }

        private void proccessNode(ai.Node node, ai.Scene scene, MeshLoader loader)
        {
            foreach (int index in node.MeshIndices)
            {
                ai.Mesh mesh = scene.Meshes[index];
                Meshes.Add(loader(mesh, scene));
            }

            foreach (ai.Node child in node.Children)
                proccessNode(child, scene, loader);
        }

        private Mesh<Vertex> proccessMesh(ai.Mesh mesh, ai.Scene scene)
        {
            //Vertex[] vertices = new Vertex[mesh.VertexCount];
            //uint[] indices = new uint[mesh.FaceCount * 3];
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            ImageTexture[] textures;

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex v = new Vertex();

                ai.Vector3D mvp = mesh.Vertices[i];
                v.Position = new vec3(mvp.X, mvp.Y, mvp.Z);

                ai.Vector3D mvn = mesh.Normals[i];
                v.Normal = new vec3(mvn.X, mvn.Y, mvn.Z);

                ai.Vector3D mvt = mesh.Tangents[i];
                v.Tangent = new vec3(mvt.X, mvt.Y, mvt.Z);

                ai.Vector3D mvb = mesh.BiTangents[i];
                v.Bitangent = new vec3(mvb.X, mvb.Y, mvb.Z);

                if (mesh.TextureCoordinateChannelCount > 0)
                {
                    ai.Vector3D mvc = mesh.TextureCoordinateChannels[0][i];
                    v.TexCoords = new vec2(mvc.X, mvc.Y);
                }
                else
                    v.TexCoords = new vec2(0, 0);

                vertices.Add(v);
            }

            for (int i = 0; i < mesh.FaceCount; i++)
                for (int j = 0; j < mesh.Faces[i].IndexCount; j++)
                    indices.Add((uint)mesh.Faces[i].Indices[j]);

            if (mesh.MaterialIndex >= 0)
            {
                ai.Material material = scene.Materials[mesh.MaterialIndex];

                List<ImageTexture> tempTextures = new List<ImageTexture>();

                ImageTexture[] diffuse = loadMaterialTextures(material, ai.TextureType.Diffuse, TextureType.Diffuse);
                tempTextures.AddRange(diffuse);

                ImageTexture[] specular = loadMaterialTextures(material, ai.TextureType.Specular, TextureType.Specular);
                tempTextures.AddRange(specular);

                ImageTexture[] normal = loadMaterialTextures(material, ai.TextureType.Height, TextureType.Normal);
                tempTextures.AddRange(normal);

                textures = tempTextures.ToArray();
            }
            else
                textures = new ImageTexture[0];

            return new ModelMesh(vertices.ToArray(), indices.ToArray(), textures);
        }

        private ImageTexture[] loadMaterialTextures(ai.Material material, ai.TextureType type, TextureType textureType)
        {
            ImageTexture[] textures = new ImageTexture[material.GetMaterialTextureCount(type)];

            for (int i = 0; i < textures.Length; i++)
            {
                ai.TextureSlot slot;
                material.GetMaterialTexture(type, i, out slot);

                string path = directory + slot.FilePath;

                ImageTexture texture;

                ImageTexture loaded = loadedTextures.Find(x => x.path == path);

                if (loaded != null)
                    texture = loaded;
                else
                {
                    texture = new ImageTexture(gl, path, textureType);
                    loadedTextures.Add(texture);
                    Console.WriteLine($"Texture file path: {path}");
                }

                textures[i] = texture;
            }

            return textures;
        }

        private Mesh<Vertex> proccessCollisionMesh(ai.Mesh mesh, ai.Scene scene)
        {
            List<vec3> vertices = new List<vec3>();
            List<uint> indices = new List<uint>();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                ai.Vector3D mvp = mesh.Vertices[i];
                vec3 v = new vec3(mvp.X, mvp.Y, mvp.Z);
                /*if(!vertices.Contains(v))*/
                vertices.Add(v);
            }

            for (int i = 0; i < mesh.FaceCount; i++)
                for (int j = 0; j < mesh.Faces[i].IndexCount; j++)
                    indices.Add((uint)mesh.Faces[i].Indices[j]);

            return new CollisionMesh(vertices.ToArray(), indices.ToArray());
        }

        public void Draw(Shader shader, int count = 1)
        {
            // meshes[0].Draw(shader);
            // return;

            foreach (ModelMesh mesh in Meshes)
                mesh.Draw(shader, count);
        }
    }
}
