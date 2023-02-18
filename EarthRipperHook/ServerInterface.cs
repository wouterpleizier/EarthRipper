using System;

namespace EarthRipperHook
{
    [Serializable]
    public class ServerInterface : MarshalByRefObject
    {
        public void ReportMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        public void ReportMessage(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public void ReportException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        public void Ping()
        {
        }
    }
}
