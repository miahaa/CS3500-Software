using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkingLibrary;
using System.Threading.Tasks;
namespace NetworkingTest
{
    [TestClass]
    public class NetworkingUnitTests
    {
         [TestMethod]
        public async Task ServerAndClientCommunicationTestAsync()
        {
            // Arrange
            string receivedMessage = "";
            var server = new Networking(new NullLogger<Networking>(), null, null, (channel, message) =>
            {
                receivedMessage = message;
            });

            var client = new Networking(new NullLogger<Networking>(), null, null, (channel, message) =>
            {
                
            });

            var port = 12345;
            var messageToSend = "Hello from client!";

            // Act
            await server.WaitForClientsAsync(port, infinite: false);
            await client.ConnectAsync("127.0.0.1", port);
            await client.SendAsync(messageToSend);

            // Assert
            Assert.AreEqual(messageToSend, receivedMessage);
        }

         [TestMethod]
        public async Task ClientDisconnectsServerNotifiedTestAsync()
        {
            // Arrange
            bool serverDisconnected = false;
            var server = new Networking(new NullLogger<Networking>(), null, (channel) =>
            {
                serverDisconnected = true;
            }, (channel, message) =>
            {
                // Not needed for this test
            });

            var client = new Networking(new NullLogger<Networking>(), null, null, (channel, message) =>
            {
                // Not needed for this test
            });

            var port = 12345;

            // Act & Assert
            await server.WaitForClientsAsync(port, infinite: false);
            await client.ConnectAsync("127.0.0.1", port);
            client.Disconnect();

            // Give some time for server to handle disconnection
            await Task.Delay(100);

            Assert.IsTrue(serverDisconnected);
        }
    }
}
