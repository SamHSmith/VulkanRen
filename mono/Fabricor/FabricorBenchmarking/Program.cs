using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Fabricor.Main.Logic.Physics.State;

namespace FabricorBenchmarking
{
    [MemoryDiagnoser]
    public class MainClass
    {
        NativeMemory<RigidbodyState> memory1;
        NativeMemory<RigidbodyState> memory2;

        [Params(100,1000,10000,100000)]
        public int Size { get; set; }

        public unsafe MainClass()
        {
            memory1 = new NativeMemory<RigidbodyState>(Size);
            memory2 = new NativeMemory<RigidbodyState>(Size);

            Random r = new Random();
            Span<RigidbodyState> span = memory1.GetSpan();
            for (int i = 0; i < span.Length; i++)
            {
                span[i].mass = (float)r.NextDouble();
            }
        }
        [Benchmark]
        public void  NativeMemCopy()
        {

            memory1.CopyTo(memory2);

        }

        public void CleanUp()
        {
            memory1.Free();
            memory2.Free();
        }

        public static void Main(string[] args)
        {
            MainClass main = new MainClass();
            BenchmarkRunner.Run<MainClass>();
            main.CleanUp();
        }
    }
}
