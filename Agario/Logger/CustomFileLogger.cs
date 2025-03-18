using Microsoft.Extensions.Logging;

/// <summary> 
/// Author:    Ha Thu 
/// Author:    Amber Tran
/// Date:      3/29/2024 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Amber T and Ha Thu - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Amber Tran, Ha Thu, certify that I wrote this code from scratch and did not copy it in 
/// part or whole from another source.  All references used in the completion of the assignment are 
/// cited in my README file. 
/// 
/// Class Contents 
/// This class represents an abstract, reusable network communication channel, which provides the 
/// ability for network communication between servers and channels.Aswell, data can be transferred 
/// between multiple connections asynchronously,and have actions taken in the server recorded through 
/// a logging mechanism.
/// 
/// 
/// </summary>
/// 

namespace Logger
{
    internal class CustomFileLogger : ILogger
    {
        private readonly string _FileName;

        public CustomFileLogger(string categoryName)
        {
            _FileName = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData)
                + Path.DirectorySeparatorChar
                + $"CS3500-{categoryName}.log";
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
            {
                File.AppendAllText(_FileName, formatter(state, exception));
            }
        }
    }
}
