using System;
using System.Collections.Generic;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Logic.Physics.State;

namespace Fabricor.Main.Logic.Physics
{
    public class RigidbodyHandle : IShapeRoot
    {
        internal uint handle;
        //private Simulation s; TODO Make Simulation a instance type
        public Span<RigidbodyState> state { get { return Simulation.GetRBState(this); } }
        public Span<RigidbodyState> interpolatedState { get { return Simulation.GetRBInterpolatedState(this); } }
        public IShape shape = null;

        public RigidbodyHandle(uint handle)
        {
            this.handle = handle;
        }

        public BoundSphere GetBound()
        {
            return shape.ToBoundSphere();
        }

        public void AddShape(IShape s)
        {
            s.root = this;
            shape = s;
        }
    }
}
