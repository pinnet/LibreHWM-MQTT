using LibreHardwareMonitor.Hardware;
using MQTTnet;
using MQTTnet.Samples.Server;
using MQTTnet.Server;
using System.Runtime.CompilerServices;
namespace LibreHWM_MQTT
{
    public sealed class WindowsBackgroundService(
        HWmonitorService HWMService,
        ILogger<WindowsBackgroundService> logger) : BackgroundService
    {
        MessageCache messageCache = new MessageCache();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string topic = "";
            IList<IHardware> hardware = HWMService.Monitor();
            var mqttFactory = new MqttFactory();
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
           
            try
            {
                using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
                {
                    await mqttServer.StartAsync();
                    mqttServer.ClientConnectedAsync += async e =>
                    {
                        messageCache.ClearMessages();
                        await Task.CompletedTask;
                    };

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        for (int i = 0; i < hardware.Count; i++)
                        {
                            hardware[i].Update();   
                            switch (hardware[i].HardwareType)
                            {
                                case HardwareType.Cpu:
                                    topic = "CPU/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.Motherboard:
                                    topic = "Motherboard/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.GpuIntel:
                                    topic = "GPU/Intel/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.GpuNvidia:
                                    topic = "GPU/Nvidia/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.GpuAmd:
                                    topic = "GPU/AMD/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.Storage:
                                    topic = "Storage/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.Network:
                                    topic = "Network/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                case HardwareType.Memory:
                                    topic = "Memory/" + hardware[i].Name;
                                    await publishAsync(mqttServer, topic);
                                    break;
                                default:
                                    topic = "";
                                    break;
                            }
                            
                            if (hardware[i].Sensors.Length > 0 && topic != "")
                            {
                                for (int j = 0; j < hardware[i].Sensors.Length; j++)
                                {
                                    string? value = hardware[i].Sensors[j].Value.ToString();
                                    string subtopic = topic + "/" + hardware[i].Sensors[j].Name.Replace('/', '-');
                                    await publishAsync(mqttServer, subtopic, value != null ? value : "");
                                }
                            }
                            if (hardware[i].SubHardware.Length > 0 && topic != "")
                            {

                                for (int k = 0; k < hardware[i].SubHardware.Length; k++)
                                {
                                    string subtopic = topic + "/" + hardware[i].SubHardware[k].Name.Replace('/', '-');
                                    hardware[i].SubHardware[k].Update();
                                    await publishAsync(mqttServer, subtopic);
                                    for (int l = 0; l < hardware[i].SubHardware[k].Sensors.Length; l++)
                                    {
                                        string? value = hardware[i].SubHardware[k].Sensors[l].Value.ToString();
                                        string subsubtopic = subtopic + "/" + hardware[i].SubHardware[k].Sensors[l].Name.Replace('/','-');
                                        await publishAsync(mqttServer, subsubtopic, value != null ? value : "");
                                    }   
                                }
                            }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                    HWMService.CloseComputer();
                    await mqttServer.StopAsync();

                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unhandled exception occurred.");
                Console.WriteLine("Message: {0}", ex.Message);  
                logger.LogError(ex, "{Message}", ex.Message);
                
                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
            
        }
        private async Task publishAsync(MqttServer server,string topic,string payload = "") {
            //Console.WriteLine("Publishing message '{0}' to topic '{1}'", payload, topic);
            topic = topic.Replace("#", "_");
            topic = topic.Replace("+", "");
            
            if (messageCache.Messages.Count > 2000)
            {
                messageCache.ClearMessages();
            }

            if (!messageCache.ContainsMessage(topic, payload))
            {
                

                messageCache.AddMessage(topic, payload);
                var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();
                await server.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = "server"
                    });
            }

           
        }
    }
}
