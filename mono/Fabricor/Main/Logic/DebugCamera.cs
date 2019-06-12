using System;
using System.Numerics;
using OpenTK.Input;

namespace Fabricor.Main.Logic
{
    public class DebugCamera : IUpdatable
    {
        public Transform transform = new Transform();

        private float rotx = 0, roty = 0;
        private int lastx = 0, lasty = 0;

        private const float Sensitivity= 0.3f;

        public void Update(float delta)
        {
            MouseState mouse = Mouse.GetState();

            rotx += Sensitivity*(float)(lastx - mouse.Y);
            roty += Sensitivity * (float)(lasty - mouse.X);

            lastx = mouse.Y;
            lasty = mouse.X;

            if (rotx < -90)
                rotx = -90;

            if (rotx > 90)
                rotx = 90;



            transform.rotation = Quaternion.Multiply(Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 180 * roty),
                Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI/180*rotx));

            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Key.W))
            {
                transform.position += Vector3.Transform(new Vector3(0, 0, -1)*delta, transform.rotation);
            }
            if (keyboard.IsKeyDown(Key.S))
            {
                transform.position += Vector3.Transform(new Vector3(0, 0, 1) * delta, transform.rotation);
            }
            if (keyboard.IsKeyDown(Key.D))
            {
                transform.position += Vector3.Transform(new Vector3(1, 0, 0) * delta, transform.rotation);
            }
            if (keyboard.IsKeyDown(Key.A))
            {
                transform.position += Vector3.Transform(new Vector3(-1, 0, 0) * delta, transform.rotation);
            }

            if (keyboard.IsKeyDown(Key.Space))
            {
                transform.position += Vector3.Transform(new Vector3(0, 1, 0) * delta, transform.rotation);
            }
            if (keyboard.IsKeyDown(Key.ShiftLeft))
            {
                transform.position += Vector3.Transform(new Vector3(0, -1, 0) * delta, transform.rotation);
            }
        }
    }
}
