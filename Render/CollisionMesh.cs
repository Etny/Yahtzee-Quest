using GlmSharp;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Yahtzee.Game;
using Yahtzee.Main;

namespace Yahtzee.Render
{
    class CollisionMesh : Mesh
    {
        public vec3[] CollisionVertices;
        public vec3[] DrawVertices;

        public int highlight = -1;

        private Shader shader;
        public ModelEntity Parent;

        public bool Overlapping = false;

        public CollisionMesh(vec3[] vertices, uint[] indices, ModelEntity parent = null) : base()
        {
            this.DrawVertices = vertices;
            this.Indices = indices;
            this.Parent = parent;

            this.shader = new Shader("Debug/Line/line");

            List<vec3> temp = new List<vec3>();

            foreach(vec3 v in vertices)
                if (!temp.Contains(v)) temp.Add(v);

            CollisionVertices = temp.ToArray();

            setupCollisionMesh();
        }

        [Conditional("DEBUG")]
        protected unsafe void setupCollisionMesh()
        {
            VBO = gl.GenBuffer();
            if (Indices != null) EBO = gl.GenBuffer();
            VAO = gl.GenVertexArray();

            gl.BindVertexArray(VAO);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            fixed (void* i = &DrawVertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(DrawVertices.Length * sizeof(vec3)), i, BufferUsageARB.StaticDraw);

            if (Indices != null)
            {
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
                fixed (void* i = &Indices[0])
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(vec3), (void*)0);
            gl.EnableVertexAttribArray(0);
        }

        public override unsafe void Draw(Shader shader) { }

        [Conditional("DEBUG")]
        public unsafe void DrawOutline()
        {
            shader.Use();
            shader.SetMat4("model", Parent.Transform.ModelMatrix);
            shader.SetBool("overlapping", Overlapping);

            gl.Disable(EnableCap.CullFace);
            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line);
            gl.LineWidth(5);

            gl.BindVertexArray(VAO);
            if (Indices != null) gl.DrawElements(PrimitiveType.Triangles, (uint)DrawVertices.Length, DrawElementsType.UnsignedInt, (void*)0);
            else gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)DrawVertices.Length);

            if(highlight >= 0)
            {
                gl.PointSize(40);
                shader.SetBool("overlapping", true);
                gl.DrawArrays(PrimitiveType.Points, highlight, 1);
            }

            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
            gl.Enable(EnableCap.CullFace);

        }

        public void UpdateHighlight(vec3 dir)
        {
            highlight = Program.PhysicsManager.Collisions.SingleSupportIndex(Parent, dir);
        }

        public RectangleF GetRectangle()
        {
            Transform t = Parent.Transform;
            return new RectangleF(t.Translation.x - (t.Scale.x / 2),
                                  t.Translation.y - (t.Scale.y / 2),
                                  t.Scale.x,
                                  t.Scale.y);
        }

        public void CheckCollision(CollisionMesh mesh)
        { 
            Overlapping = Program.PhysicsManager.Collisions.GJK(Parent, mesh.Parent);
        }
    }
}
