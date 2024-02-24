using EarthRipperShared;

namespace EarthRipper
{
    internal static class Log
    {
        private static readonly ConsoleColor _defaultColor;
        private static readonly object _lock;

        private static SharedTextBuffer? _sharedTextBuffer;
        private static CancellationTokenSource? _canceller;

        static Log()
        {
            _defaultColor = Console.ForegroundColor;
            _lock = new object();
        }

        internal static void Initialize(string pipeName)
        {
            _sharedTextBuffer = new SharedTextBuffer(pipeName);
            _canceller = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!_canceller.IsCancellationRequested)
                {
                    string? text = _sharedTextBuffer.ReadAll();
                    if (!string.IsNullOrEmpty(text))
                    {
                        foreach (string line in text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (line.StartsWith(LogUtil.NativeMessageID))
                            {
                                Native(line.Substring(LogUtil.NativeMessageID.Length));
                            }
                            else if (line.StartsWith(LogUtil.InformationMessageID))
                            {
                                Information(line.Substring(LogUtil.InformationMessageID.Length));
                            }
                            else if (line.StartsWith(LogUtil.WarningMessageID))
                            {
                                Warning(line.Substring(LogUtil.WarningMessageID.Length));
                            }
                            else if (line.StartsWith(LogUtil.ErrorMessageID))
                            {
                                Error(line.Substring(LogUtil.ErrorMessageID.Length));
                            }
                        }
                    }

                    if (!_canceller.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            , _canceller.Token).ConfigureAwait(false);
        }

        public static void Native(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(message);
                Console.ForegroundColor = _defaultColor;
            }
        }

        public static void Information(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message);
                Console.ForegroundColor = _defaultColor;
            }
        }

        public static void Warning(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ForegroundColor = _defaultColor;
            }
        }

        public static void Error(Exception exception)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ForegroundColor = _defaultColor;
            }
        }

        public static void Error(string message, Exception? exception = null)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                if (exception != null)
                {
                    Console.WriteLine(exception);
                }
                Console.ForegroundColor = _defaultColor;
            }
        }
    }
}
