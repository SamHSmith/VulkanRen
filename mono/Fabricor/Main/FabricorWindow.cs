﻿using System;
using Fabricor.Main.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace Fabricor.Main
{
    public class FabricorWindow : GameWindow
    {
        public FabricorWindow(int display, int windowmode, int width, int height, string name)
        : base(width, height, GraphicsMode.Default, name, (GameWindowFlags)windowmode, DisplayDevice.GetDisplay((DisplayIndex)display), 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            MasterRenderer.Init();
        }

        protected override void Dispose(bool manual)
        {

            base.Dispose(manual);
            MasterRenderer.CleanUp();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            MasterRenderer.MasterRender((float)e.Time);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }
    }
}
