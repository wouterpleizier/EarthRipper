using EarthRipperShared;
using System.Collections.Concurrent;
using System.Text;

namespace EarthRipperHook
{
    internal static class Log
    {
        private static ConcurrentQueue<string>? _messageQueue;
        private static SharedTextBuffer? _sharedTextBuffer;
        private static CancellationTokenSource? _canceller;

        internal static void Initialize(string pipeName)
        {
            _messageQueue = new ConcurrentQueue<string>();
            _sharedTextBuffer = new SharedTextBuffer(pipeName);
            _canceller = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!_canceller.IsCancellationRequested)
                {
                    StringBuilder messages = new StringBuilder();
                    while (_messageQueue.TryDequeue(out string? message))
                    {
                        messages.Append(message);
                    }

                    if (messages.Length > 0)
                    {
                        _sharedTextBuffer.Append(messages.ToString());
                    }

                    if (!_canceller.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            , _canceller.Token).ConfigureAwait(false);
        }

        internal static void Native(params string?[] messages) => Enqueue(LogUtil.NativeMessageID, messages);
        internal static void Information(params string?[] messages) => Enqueue(LogUtil.InformationMessageID, messages);
        internal static void Warning(params string?[] messages) => Enqueue(LogUtil.WarningMessageID, messages);
        internal static void Error(params string?[] messages) => Enqueue(LogUtil.ErrorMessageID, messages);
        internal static void Error(Exception? exception) => Enqueue(LogUtil.ErrorMessageID, exception?.ToString());

        private static void Enqueue(string? prefix, params string?[] messages)
        {
            if (_messageQueue != null && messages != null && messages.Length > 0)
            {
                StringBuilder stringBuilder = new StringBuilder(prefix);
                foreach (string? message in messages)
                {
                    stringBuilder.AppendLine(message);
                }

                _messageQueue.Enqueue(stringBuilder.ToString());
            }
        }
    }
}
