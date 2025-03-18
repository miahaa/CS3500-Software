
```
Author:     Ha Thu and Amber Tran
Partner:    None
Start Date: 31-March-2024
Course:     CS 3505, University of Utah, School of Computing
Repo:       https://github.com/uofu-cs3500-spring24/assignment-seven-logging-and-networking-am_mi
Commit Date: 31-March-2024 
Solution:   Networking
Copyright:  CS 3500 and Ha Thu and Amber Tran - This work may not be copied for use in Academic Coursework.
```

# Overview of the Networking functionality

This solution is designed to facilitate the development of a chat program comprising a client application (ChatClient), a server application (ChatServer), a networking support library (NetworkingLibrary), and a file logging component (LoggerLibrary).

## Projects in the Solution
### ChatClient
#### Description: This project contains the graphical user interface (GUI) for the chat client application.
#### Functionality:
- Allows users to connect to a server, send messages, and receive messages from other connected clients.
- Displays the list of participants currently connected to the server.
- Provides status information such as connection status (Connected/Disconnected/Error).

### ChatServer
#### Description: This project hosts the chat server application responsible for managing client connections and message distribution.
#### Functionality:
- Allows the system administrator to start and manage the chat server.
- Handles multiple client connections concurrently, each on a separate thread.
- Broadcasts messages received from clients to all connected clients.

### NetworkingLibrary
#### Description: This class library provides an abstraction layer for networking communications.
#### Functionality:
- Defines the Networking class, which encapsulates the functionality for establishing and managing network connections.
- Supports asynchronous methods for handling incoming data, sending data, and managing client connections.

### LoggerLibrary
#### Description: This class library implements a custom file logging component.
#### Functionality:
- Provides logging functionality by writing log messages to a file.
- Integrates with the Microsoft.Extensions.Logging library for logging management.

# Testing
Testing the system involves verifying that the NetworkingLibrary functions as intended, ensuring that both the server and client components interact correctly. First, I set up unit tests using MSTest to cover various scenarios, such as establishing connections, sending messages, and handling disconnections. For instance, I create tests to simulate a client connecting to a server, sending a message, and validating that the server receives it. Additionally, I design tests to handle edge cases, like disconnecting unexpectedly or handling multiple client connections concurrently. 

# Time Expenditures:

    1. Assignment Seven:   Predicted Hours:          24        Actual Hours:   32
        - Ha Thu: 32
        - Amber Tran: 25
  

