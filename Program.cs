using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

Console.WriteLine("Reverse proxy!");

// Configuración 
var configuration = new ConfigurationBuilder()
                            .AddJsonFile("./settings.json", false, true)
                            .Build();    

var settings = configuration.Get<Settings>();

IPEndPoint from = new IPEndPoint(IPAddress.Parse(settings!.config.listen.ip), settings.config.listen.port);
IPEndPoint[] to = settings.config.forwards.Select(x=> new IPEndPoint(IPAddress.Parse(x.Split(":")[0]), int.Parse(x.Split(":")[1]))).ToArray();


// DI 
var serviceProvider = new ServiceCollection()
                        .AddOptions()
                        .AddLogging( conf => { 
                                        conf.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Simple);
                                        conf.AddSimpleConsole(options => {
                                            options.IncludeScopes = true; 
                                            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                                            options.UseUtcTimestamp = false;
                                        });
                                        conf.SetMinimumLevel(LogLevel.Trace);
                                    })
                        .AddTransient<ReverseProxy>()
                        .AddTransient<PortMap>(provider => {
                            var logger = provider.GetRequiredService<ILogger<PortMap>>();
                            return new PortMap(logger, from, to);
                        })
                        .AddTransient<Forwarder>()
                        .BuildServiceProvider();



var logger = serviceProvider.GetService<ILogger<Program>>()!;

// Starts Reverse Proxy 
var proxy = serviceProvider.GetService<ReverseProxy>();
if (proxy != null) 
    await proxy.StartForwarder();


logger.LogInformation("Fin");