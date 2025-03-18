
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

Commits are mostly made from Ha Thu's GitHub account since we are both working on her computer.

# Assignment Specific Topics

The ChatClient project fulfills the requirements outlined in the assignment, providing a Graphical User Interface (GUI) for connecting to a chat server, sending and receiving messages, and viewing the list of participants. It integrates the NetworkingLibrary for network communication and incorporates logging functionality from the LoggerLibrary.

### Functionality

- Allows users to specify the remote machine name, their name, and send messages to the server.
- Displays the list of participants currently connected to the server.
- Shows the conversation history with messages received from other clients.
- Indicates the status of the connection (Connected/Disconnected/Error).

### Implementation Details

- Developed using the MAUI framework for cross-platform compatibility.
- Utilizes the NetworkingLibrary to establish and manage connections with the chat server.
- Implements event handlers to capture user actions and interact with the Networking object.
- Incorporates logging functionality from the LoggerLibrary to record events and errors during runtime.

# Consulted Peers:
None

# References:

1. Microsoft Documentation - [MAUI Framework](https://docs.microsoft.com/en-us/maui/)