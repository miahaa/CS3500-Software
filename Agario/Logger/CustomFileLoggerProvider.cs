using Microsoft.Extensions.Logging;

namespace Logger
{
    public class CustomFileLoggerProvider : ILoggerProvider
    {

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(categoryName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
