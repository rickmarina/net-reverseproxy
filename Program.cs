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

string raw = """
GET / HTTP/1.1
Host: 127.0.0.1:8787
Connection: keep-alive
sec-ch-ua: "Google Chrome";v="129", "Not=A?Brand";v="8", "Chromium";v="129"
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: "Windows"
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Sec-Fetch-Site: none
Sec-Fetch-Mode: navigate
Sec-Fetch-User: ?1
Sec-Fetch-Dest: document
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: es-ES,es;q=0.9


""";
var parser = new HttpRawParser(raw);

Console.WriteLine(parser);


return; 

// Starts Reverse Proxy 
var proxy = serviceProvider.GetService<ReverseProxy>();
if (proxy != null) 
    await proxy.StartForwarder();


logger.LogInformation("Fin");