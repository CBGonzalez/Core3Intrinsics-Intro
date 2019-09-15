using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core3IntrinsicsBenchmarks
{
    public unsafe class AlignedMemoryHandle<T> where T : struct
    {
        private MemoryHandle memoryHandle;
        readonly byte* bytePointer;
        readonly int byteArrayLength;
        readonly Memory<T> memory;

        public MemoryHandle MemoryHandle => memoryHandle;

        public ref byte ByteRef => ref GetByteRef();

        public ref T TRef => ref GetTRef();

        public Memory<T> Memory => memory;

        public int ByteArrayLength => byteArrayLength;

        public unsafe AlignedMemoryHandle(void* pointer, GCHandle handle, ref T arrayStart, int byteLength)
        {
            memoryHandle = new MemoryHandle(pointer, handle);
            bytePointer = (byte*)pointer;
            ref T tRef = ref arrayStart;
            byteArrayLength = byteLength;
            memory = new Memory<T>(MemoryMarshal.Cast<byte, T>(new Span<byte>(pointer, byteLength)).ToArray());
        }

        private unsafe ref byte GetByteRef()
        {
            return ref bytePointer[0];
        }

        private unsafe ref T GetTRef()
        {
            return ref MemoryMarshal.Cast<byte, T>(new Span<Byte>((void*)bytePointer, byteArrayLength)).ToArray()[0];
        }

    }
}
