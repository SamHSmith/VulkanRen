using System;
using Fabricor.Main;

namespace Fabricor
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            new FabricorWindow(0, 0, 1600, 900, "Fabricor 0.2").Run(0);
        }
    }
}
