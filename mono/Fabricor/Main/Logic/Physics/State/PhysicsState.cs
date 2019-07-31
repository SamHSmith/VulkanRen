using System;
using System.Runtime.InteropServices;

namespace Fabricor.Main.Logic.Physics.State
{
    public class PhysicsState
    {
        private NativeMemory<RigidbodyState> state;

        public Span<RigidbodyState> State { get { return state.GetSpan(); } }

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

            state.Free();
        }
    }
}
