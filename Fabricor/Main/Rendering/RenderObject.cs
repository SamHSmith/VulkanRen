using System;
using Fabricor.Main.Logic;
using Fabricor.Main.Rendering.Models;

namespace Fabricor.Main.Rendering
{
    public class RenderObject
    {
        public Transform Transform { get; private set; }
        public TexturedModel Model { get; private set; }

        public RenderObject(Transform transform, TexturedModel model)
        {
            Transform = transform;
            Model = model;
        }
    }
}
