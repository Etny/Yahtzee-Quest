﻿
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;

namespace Yahtzee.Render.Models
{
    class ModelManager
    {
        private static Dictionary<string, List<ModelEntity>> Entities = new Dictionary<string, List<ModelEntity>>();

        public static void Register(ModelEntity e, string key)
        {
            if (!Entities.ContainsKey(key))
                Entities.Add(key, new List<ModelEntity>());

            Entities.GetValueOrDefault(key).Add(e);
        }

        public static void Deregister(ModelEntity e, string key)
        {
            if (!Entities.ContainsKey(key)) return;

            Entities.GetValueOrDefault(key).Remove(e);
            if (Entities.GetValueOrDefault(key).Count <= 0) Entities.Remove(key);
        }

        public static void DrawModels(Shader shader)
        {

            foreach (var e in Entities.Values)
            {
                var l = e.FindAll(m => m.DrawInstanced && !m.Hide);
                for (int i = 0; i < l.Count; i += 100)
                {
                    int amount = l.Count - i <= 100 ? l.Count - i : 100;

                    for (int c = 0; c < amount; c++)
                        shader.SetMat4($"models[{c}]", l[i + c].Transform.ModelMatrix);

                    l[0].Model.Draw(shader, amount);
                }
            }
        }

    }
}
