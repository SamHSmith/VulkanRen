using System;
using System.Runtime.InteropServices;

namespace Fabricor.Main.Logic.Physics.State
{
    public class PhysicsState
    {
        private NativeMemory<RigidbodyState> state;

        public Span<RigidbodyState> State { get { return state.GetSpan(); } }

        public PhysicsState(int InitialCapacity)
        {

            state = new NativeMemory<RigidbodyState>(InitialCapacity);

            state.Free();
        }
    }
}
