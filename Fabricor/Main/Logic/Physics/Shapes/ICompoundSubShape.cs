﻿using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public interface ICompoundSubShape:IShape
    {
        float Mass { get; set; }
        Vector3 Localposition { get; set; }
        void UpdateBound();
    }
}