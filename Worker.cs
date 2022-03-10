namespace MQTT.PUB;

using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttConnectionString = Environment.GetEnvironmentVariable("MQTTHost") ?? "vernmq";
        await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
                                .WithTcpServer(mqttConnectionString)
                                .WithNoKeepAlive()
                                .Build(),
                                CancellationToken.None
                        );
        if (!mqttClient.IsConnected)
        {
            _logger.LogError("Not Connected To MQTT");
            stoppingToken.ThrowIfCancellationRequested();
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                                                .WithTopic("test")
                                                .WithPayload($"{DateTime.Now.ToString()}")
                                                .Build(),
                                                stoppingToken
                                            );

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
