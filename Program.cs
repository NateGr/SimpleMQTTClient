using System;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace SimpleMQTTClient
{
    class Program
    {
        private static readonly string encryptionKey = "aP3x!9sD#4fG7hJkL0mN2qR5tV8wXzY1"; // 32 bytes for AES-256
        private static readonly string encryptionIV = "bC6dE8fG1hJ2kL3m"; // 16 bytes for AES

        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SimpleMQTTClient <encrypt|pub> <username> <password> <host> <topic> <message>");
                return;
            }

            string mode = args[0];

            if (mode == "encrypt")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: SimpleMQTTClient encrypt <username> <password>");
                    return;
                }

                string username = args[1];
                string password = args[2];

                string encryptedUsername = EncryptString(username);
                string encryptedPassword = EncryptString(password);

                Console.WriteLine($"Encrypted Username: {encryptedUsername}");
                Console.WriteLine($"Encrypted Password: {encryptedPassword}");
            }
            else if (mode == "pub")
            {
                if (args.Length < 6)
                {
                    Console.WriteLine("Usage: SimpleMQTTClient pub <encryptedUsername> <encryptedPassword> <host> <topic> <message>");
                    return;
                }

                string encryptedUsername = args[1];
                string encryptedPassword = args[2];
                string host = args[3];
                string topic = args[4];
                string message = args[5];

                string username = DecryptString(encryptedUsername);
                string password = DecryptString(encryptedPassword);

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
            }
            else
            {
                Console.WriteLine("Invalid mode. Use 'encrypt' or 'pub'.");
            }
        }

        private static string EncryptString(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] iv = Encoding.UTF8.GetBytes(encryptionIV);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        private static string DecryptString(string encryptedText)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] iv = Encoding.UTF8.GetBytes(encryptionIV);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}