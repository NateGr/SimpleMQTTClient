using System;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Diagnostics;

namespace MyMqttApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("homeassistant.local")
                .WithCredentials("mqttuserext", "Starbug1!")
                .Build();

            await mqttClient.ConnectAsync(options);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("homeassistant/customaction/execute")
                .WithPayload("toggle_light|light.master_bedroom_lamp")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();


            await mqttClient.PublishAsync(message);

            await mqttClient.DisconnectAsync();
        }
    }
}
