using System;

namespace WaveChart
{
    public static class ExceptionHandler
    {
        public static void HandleException(string message)
        {
            Console.WriteLine(message);
            throw new Exception(message);
        }
        
        public static void HandleExceptionWOStop(string message)
        {
            Console.WriteLine(message);
        }
    }
}