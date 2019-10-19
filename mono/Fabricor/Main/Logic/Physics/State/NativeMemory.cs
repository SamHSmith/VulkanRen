using System;
using System.Runtime.InteropServices;

namespace Fabricor.Main.Logic.Physics.State
{
    public class NativeMemory<T> where T : unmanaged
    {
        public IntPtr ptr { get; private set; }
        public int length;
        public int byteCount;

        public unsafe NativeMemory(int capacity)
        {
            length = capacity;
            byteCount = Marshal.SizeOf(default(T)) * length;
            ptr = Marshal.AllocHGlobal(byteCount);


            T* myptr = (T*)this.ptr;

            for (int i = 0; i < length; i++)
            {
                *myptr = new T();
                myptr++;
            }
        }

        public unsafe Span<T> GetSpan()
        {
            return new Span<T>(ptr.ToPointer(), length);
        }

        public unsafe bool CopyTo(NativeMemory<T> other)
        {
            if (other.length < this.length)
                return false;

            T* otherptr = (T*)other.ptr;
            T* myptr = (T*)this.ptr;


            for (int i = 0; i < this.length; i++)
            {
                *otherptr = *myptr;
                myptr++;
                otherptr++;
            }
            return true;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
