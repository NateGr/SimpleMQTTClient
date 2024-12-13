using System;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;

namespace MyMqttApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: MyMqttApp <username> <password> <host> <topic> <message>");
                return;
            }

            string username = args[0];
            string password = args[1];
            string host = args[2];
            string topic = args[3];
            string message = args[4];

            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host)
                .WithCredentials(username, password)
                .Build();

            await mqttClient.ConnectAsync(options);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);

            await mqttClient.DisconnectAsync();

            Console.WriteLine("Message published successfully!");
        }
    }
}
