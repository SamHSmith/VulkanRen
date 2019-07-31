using System;
using System.Collections.Generic;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Logic.Physics.State;

namespace Fabricor.Main.Logic.Physics
{
    public class RigidbodyHandle
    {
        internal uint handle;
        //private Simulation s; TODO Make Simulation a instance type
        public Span<RigidbodyState> state { get { return Simulation.GetRBState(this); } }
        public List<IShape> shapes = new List<IShape>();

        public BoundSphere GetBound()
        {
            float r = 0;
            foreach (var s in shapes)
            {
                BoundSphere b = s.ToBoundSphere();
                if (b.radius > r)
                {
                    r = b.radius;
                }
            }
            return new BoundSphere { radius = r, Rigidbody = this };
        }
    }
}
