using System;
using System.Runtime.InteropServices;

namespace Fabricor.Main.Logic.Physics.State
{
    public class PhysicsState
    {
        internal NativeMemory<RigidbodyState> state;

        public Span<RigidbodyState> Span { get { return state.GetSpan(); } }

        public unsafe Span<RigidbodyState> GetRef(uint index)
        {
            RigidbodyState* ptr = (RigidbodyState*)state.ptr;
            if (index >= state.length)
                throw new IndexOutOfRangeException("Index " + index + " is out of bounds for physics state " + state.length);
            ptr += index;
            return new Span<RigidbodyState>(ptr,1);
        }

        public PhysicsState(int InitialCapacity)
        {
            state = new NativeMemory<RigidbodyState>(InitialCapacity);
        }

        public void Grow()
        {
            NativeMemory<RigidbodyState> newstate = new NativeMemory<RigidbodyState>(state.length * 2);
            state.CopyTo(newstate);
            NativeMemory<RigidbodyState> old = state;
            state = newstate;
            old.Free();
        }

        public void CleanUp()
        {
            state.Free();
        }
    }
}
