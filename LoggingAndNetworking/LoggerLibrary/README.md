
```
Author:     Ha Thu
Partner:    Amber Tran
Course:     CS 4300, University of Utah, School of Computing
Repo:       [https://github.com/uofu-cs3500-spring24/assignment-seven-logging-and-networking-am_mi](https://github.com/uofu-cs3500-spring24/assignment-seven-logging-and-networking-am_mi)
Date:       31-March-2024
Project:    Logging and Networking
Copyright:  CS 3500 and Ha Thu and Amber Tran - This work may not be copied for use in Academic Coursework.
```

# Comments to Evaluators:

Commits are mostly made from Ha Thu github account since we are both working on her computer.

# Assignment Specific Topics
The LoggerLibrary project fulfills the requirements outlined in the assignment, providing a robust and extensible logging framework for recording application events and messages to a file. It incorporates best practices for logging and file management, ensuring reliability, scalability, and maintainability.

### Functionality

#### Custom File Logger Provider

- Implements the `ILoggerProvider` interface to enable integration with the ASP.NET Core logging framework.
- Creates and manages instances of the custom file logger class for handling log messages.

#### Custom File Logger

- Implements the `ILogger` interface to define logging behavior and methods for recording log messages.
- Supports logging messages with varying levels of severity, including Trace, Debug, Error, etc.
- Appends log messages to a designated log file with timestamp, thread ID, and message content in a standardized format.

### Implementation Details

#### Constructor

- Initializes the logger with the specified file name and path, ensuring the log file's accessibility and proper storage location.
- Configures the logger to append log messages to the designated file and format them according to the specified criteria.

#### Log Message Formatting

- Utilizes string interpolation to construct log messages with timestamp, thread ID, severity level, and message content.
- Ensures consistency and readability of log entries by adhering to a standardized format across all messages.

#### File Management

- Handles file creation and management operations, including opening, writing, and closing the log file.
- Utilizes the `Environment.SpecialFolder.ApplicationData` directory for storing log files, ensuring platform-independent access and file storage.

# Consulted Peers:
None

# References:


1. Microsoft Documentation - [ILoggerProvider Interface](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerprovider)
2. C# Logging Best Practices - [Logging Best Practices](https://blog.elmah.io/logging-best-practices-in-csharp/)
3. ASP.NET Core Logging - [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0)

