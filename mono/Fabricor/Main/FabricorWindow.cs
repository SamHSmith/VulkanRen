using System;
using Fabricor.Main.Logic;
using Fabricor.Main.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace Fabricor.Main
{
    public class FabricorWindow : GameWindow
    {
        public FabricorWindow(int display, int windowmode, int width, int height, string name)
        : base(width, height, GraphicsMode.Default, name, (GameWindowFlags)windowmode, DisplayDevice.GetDisplay((DisplayIndex)display), 4, 5, GraphicsContextFlags.ForwardCompatible)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            LogicMaster.Init();
        }

        protected override void Dispose(bool manual)
        {

            base.Dispose(manual);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            LogicMaster.CleanUp();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            LogicMaster.Update((float)e.Time);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            MasterRenderer.AspectRatio = (float)Width / Height;
            GL.Viewport(0, 0, Width, Height);
        }
    }
}
