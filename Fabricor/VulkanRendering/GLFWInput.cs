using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using GLFW;

namespace Fabricor.VulkanRendering
{
    public static class GLFWInput
    {
        private static Dictionary<Keys, float> pressTime = new Dictionary<Keys, float>();
        private static Dictionary<Keys, Action> subscribePress = new Dictionary<Keys, Action>();
        private static Dictionary<Keys, Action> subscribeRelease = new Dictionary<Keys, Action>();

        private static Queue<Keys> newlyPressed = new Queue<Keys>(), newlyReleased = new Queue<Keys>();


        private static double lastTime = Glfw.Time;
        public static void Update()
        {
            float time = (float)(Glfw.Time - lastTime);
            lastTime = Glfw.Time;
            for (int i = 0; i < newlyReleased.Count; i++)
            {
                Keys k = newlyReleased.Dequeue();
                if (pressTime.ContainsKey(k))
                {
                    pressTime[k] = 0;
                }
                else
                {
                    pressTime.Add(k, 0);
                }
                if (!subscribeRelease.ContainsKey(k))
                    subscribeRelease.Add(k, delegate { });
                subscribeRelease[k]();
            }
            for (int i = 0; i < newlyPressed.Count; i++)
            {
                Keys k = newlyPressed.Dequeue();
                if (pressTime.ContainsKey(k))
                {
                    pressTime[k] += time;
                }
                else
                {
                    pressTime.Add(k, time);
                }
                if (!subscribePress.ContainsKey(k))
                    subscribePress.Add(k, delegate { });
                subscribePress[k]();
            }
        }

        public static float TimeKeyPressed(Keys key)
        {
            if (!pressTime.ContainsKey(key))
                pressTime.Add(key, 0);
            return pressTime[key];
        }
        public static void Subscribe(Keys k, Action action, InputState state){
            if(state==InputState.Press){
                if (!subscribePress.ContainsKey(k))
                    subscribePress.Add(k, delegate { });
                subscribePress[k]+=action;
            }else if(state==InputState.Release){
                if (!subscribeRelease.ContainsKey(k))
                    subscribeRelease.Add(k, delegate { });
                subscribeRelease[k]+=action;
            }
        }

        public static void Unsubscribe(Keys k, Action action, InputState state){
            if(state==InputState.Press){
                if (!subscribePress.ContainsKey(k))
                    subscribePress.Add(k, delegate { });
                subscribePress[k]-=action;
            }else if(state==InputState.Release){
                if (!subscribeRelease.ContainsKey(k))
                    subscribeRelease.Add(k, delegate { });
                subscribeRelease[k]-=action;
            }
        }

        public static void KeyCallback(IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (state == InputState.Press)
                newlyPressed.Enqueue(key);

            if (state == InputState.Release)
                newlyReleased.Enqueue(key);
        }
    }
}