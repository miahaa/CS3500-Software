
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

## Test Cases

- we create instances of both the server and the client.

- We specify a port number on which the server will listen for incoming connections.

- We then start the server to listen for clients on the specified port.

- Next, we have the client connect to the server using the server's IP address and port number.

- After the client successfully connects, it sends a message to the server.

- The server receives the message, and the client receives a confirmation message from the server.

# Assignment Specific Topics
The NetworkingLibrary project fulfills the requirements outlined in the assignment, including the implementation of a networking library with support for dependency injection, asynchronous methods, and a callback mechanism for event reporting.

### Functionality

#### Networking Class
- Defines the `Networking` class, which acts as a wrapper around the C# `TcpClient` object and its asynchronous methods.
- Supports constructor injection for dependency management, allowing the integration of logging and callback functionality.
- Provides methods for establishing connections, sending and receiving messages, and managing client connections asynchronously.
- Implements a callback mechanism to report events such as connection establishment, disconnection, and message arrival to the calling application.

#### Asynchronous Methods
- Utilizes asynchronous programming patterns and the `async/await` keywords to handle network operations efficiently without blocking the main thread.
- Ensures responsiveness and scalability by allowing multiple network operations to execute concurrently without causing thread contention.

#### Networking Protocol
- Adheres to a simple newline-terminated message protocol for communication between clients and the server.
- Handles message encoding and decoding to ensure proper transmission and interpretation of data.

### Implementation Details

#### Constructor
- Accepts parameters for the logger object and callback delegates, facilitating dependency injection for logging and event handling.
- Initializes the `TcpClient` object and sets up event handlers for connection-related events.

#### Callback Mechanism
- Defines delegate types for reporting connection establishment, disconnection, and message arrival events.
- Stores callback methods provided by the calling application and invokes them upon the occurrence of corresponding events.

#### Threading Model
- Utilizes asynchronous methods and tasks to handle network operations on separate threads, ensuring non-blocking execution and efficient resource utilization.
- Implements thread-safe mechanisms to synchronize access to shared resources and prevent race conditions.


# Consulted Peers:
None

# References:

1. Microsoft Documentation - [TcpClient Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)
2. C# Async Programming - [Async Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
3. C# Networking Tutorial - [Networking Tutorial](https://www.tutorialspoint.com/csharp/csharp_networking.htm)

