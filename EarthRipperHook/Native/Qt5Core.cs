using Reloaded.Memory;
using Reloaded.Memory.Sources;
using System.Runtime.InteropServices;
using System.Text;

namespace EarthRipperHook.Native
{
    [FunctionLibrary("Qt5Core.dll")]
    internal static class Qt5Core
    {
        internal class QString : IDisposable
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct QStringDataHeader
            {
                public int ReferenceCount;
                public int Size;
                public uint Allocated;
                public uint Offset;
            }

            internal nuint NativeQStringData { get; }
            internal bool IsNativelyAllocated { get; }

            public QString(nuint nativeQStringData, int levelsOfIndirection = 0)
            {
                nuint address = nativeQStringData;
                for (int i = 0; i < levelsOfIndirection; i++)
                {
                    Memory.CurrentProcess.Read(address, out address);
                }

                NativeQStringData = address;
                IsNativelyAllocated = true;
            }

            public QString(string value)
            {
                QStringDataHeader header = new()
                {
                    ReferenceCount = -1,                  // -1 so Qt never frees this memory
                    Size = value.Length,                  // Number of characters
                    Allocated = (uint)(value.Length + 2), // Number of characters plus null terminator and padding(?)
                    Offset = 16                           // String data offset relative to start of header; always 16
                };

                byte[] bytes =
                [
                    .. Struct.GetBytes(header),
                    .. Encoding.Unicode.GetBytes(value),
                    .. Encoding.Unicode.GetBytes("\0"),
                ];

                NativeQStringData = Memory.CurrentProcess.Allocate(bytes.Length);
                Memory.CurrentProcess.WriteRaw(NativeQStringData, bytes);
            }

            public void Dispose()
            {
                if (NativeQStringData != 0x0 && !IsNativelyAllocated)
                {
                    Memory.CurrentProcess.Free(NativeQStringData);
                }
            }

            public override string ToString()
            {
                if (NativeQStringData == 0x0)
                {
                    Log.Warning("Attempting to read null QString");
                    return string.Empty;
                }

                Struct.FromPtr(NativeQStringData, out QStringDataHeader header);

                if (header.ReferenceCount < -1 || header.Size < 0 || header.Offset != 16)
                {
                    Log.Warning(
                        $"Unexpected QString header contents at {NativeQStringData}:",
                        $"  ReferenceCount = {header.ReferenceCount}:",
                        $"  Size           = {header.Size}:",
                        $"  Allocated      = {header.Allocated}:",
                        $"  Offset         = {header.Offset}:");

                    return string.Empty;
                }

                Memory.CurrentProcess.ReadRaw(NativeQStringData + 16, out byte[] bytes, header.Size * 2);
                return Encoding.Unicode.GetString(bytes);
            }
        }
    }
}
