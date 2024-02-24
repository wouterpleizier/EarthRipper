using System.IO.MemoryMappedFiles;
using System.Text;

namespace EarthRipperShared
{
    public class SharedTextBuffer
    {
        private const long Size = 8192;

        private MemoryMappedFile _memoryMappedFile;
        private Mutex _mutex;

        public SharedTextBuffer(string name)
        {
            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(name, Size);
            _mutex = new Mutex(false, string.Concat(name, "_MUTEX"));
        }

        public void Append(string value)
        {
            _mutex.WaitOne();
            try
            {
                using (MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream(0, Size))
                {
                    string? currentValue = GetCurrentValue(stream);
                    string newValue = string.Concat(currentValue, value);
                    int newLength = Encoding.Unicode.GetByteCount(newValue);

                    stream.Position = 0;
                    stream.Write(BitConverter.GetBytes(newLength));
                    stream.Write(Encoding.Unicode.GetBytes(newValue));
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public string? ReadAll()
        {
            string? result = null;
            _mutex.WaitOne();
            try
            {
                using (MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream(0, Size))
                {
                    result = GetCurrentValue(stream);

                    stream.Position = 0;
                    stream.Write(BitConverter.GetBytes(0));
                }
                return result;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        private string? GetCurrentValue(MemoryMappedViewStream stream)
        {
            byte[] bytes = new byte[sizeof(int)];
            stream.Position = 0;
            stream.Read(bytes, 0, bytes.Length);
            int currentLength = BitConverter.ToInt32(bytes);

            if (currentLength > 0)
            {
                bytes = new byte[currentLength];
                stream.Read(bytes, 0, bytes.Length);
                return Encoding.Unicode.GetString(bytes);
            }

            return null;
        }
    }
}
