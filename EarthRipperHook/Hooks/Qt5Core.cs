using Reloaded.Memory;
using Reloaded.Memory.Sources;
using System.Runtime.InteropServices;
using System.Text;

namespace EarthRipperHook.Hooks
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

            private readonly nuint _nativeQStringData;
            private readonly bool _isNativelyAllocated;

            public QString(nuint nativeQStringData, int levelsOfIndirection = 0)
            {
                nuint address = nativeQStringData;
                for (int i = 0; i < levelsOfIndirection; i++)
                {
                    Memory.CurrentProcess.Read(address, out address);
                }

                _nativeQStringData = address;
                _isNativelyAllocated = true;
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

                _nativeQStringData = Memory.CurrentProcess.Allocate(bytes.Length);
                Memory.CurrentProcess.WriteRaw(_nativeQStringData, bytes);
            }

            public void Dispose()
            {
                if (_nativeQStringData != 0x0 && !_isNativelyAllocated)
                {
                    Memory.CurrentProcess.Free(_nativeQStringData);
                }
            }

            public override string ToString()
            {
                if (_nativeQStringData == 0x0)
                {
                    Log.Warning("Attempting to read null QString");
                    return string.Empty;
                }

                Struct.FromPtr(_nativeQStringData, out QStringDataHeader header);

                if (header.ReferenceCount < -1 || header.Size < 0 || header.Offset != 16)
                {
                    Log.Warning(
                        $"Unexpected QString header contents at {_nativeQStringData}:",
                        $"  ReferenceCount = {header.ReferenceCount}:",
                        $"  Size           = {header.Size}:",
                        $"  Allocated      = {header.Allocated}:",
                        $"  Offset         = {header.Offset}:");

                    return string.Empty;
                }

                Memory.CurrentProcess.ReadRaw(_nativeQStringData + 16, out byte[] bytes, header.Size * 2);
                return Encoding.Unicode.GetString(bytes);
            }
        }
    }
}
