// using System.Threading.Tasks;
// using odysseyAnalytics.Connections;
// namespace odysseyAnalytics
// {
//     public class Class1
//     {
//         static async Task Main(string[] args)
//         {
//             var rabbitMqHandler = new RabbitMqHandler();
//
//             // Replace these with your RabbitMQ server details
//             string host = "localhost";
//             string username = "guest";
//             string password = "guest";
//             string queueName = "testQueue";
//             string message = "Hello, RabbitMQ!";
//
//             // Connect to RabbitMQ
//             await rabbitMqHandler.Connect(host, username, password);
//
//             // Publish a message
//             await rabbitMqHandler.PublishMessage(queueName, message);
//
//             // Close the connection
//             await rabbitMqHandler.Close();
//         }
//     }
// }
