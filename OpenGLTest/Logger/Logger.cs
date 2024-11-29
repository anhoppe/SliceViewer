using System;
using Logger.Contract;

namespace OpenGLTest.Logger
{
    internal class Logger : ILogger
    {
        public void Error(string message, Exception exception = null)
        {
            Console.WriteLine(message);
        }

        public void Info(string message, Exception exception = null)
        {
            Console.WriteLine(message);
        }

        public void Warn(string message, Exception exception = null)
        {
            Console.WriteLine(message);
        }
    }
}
