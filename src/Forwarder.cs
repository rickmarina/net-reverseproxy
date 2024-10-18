using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

internal class Forwarder
{
    public Guid Id { get; private set; }
    private bool _started = false;
    public PortMap Map { get; set; }
    public TcpListener Server { get; set; }
    public Task? ServerTask { get; private set; }
    public ConcurrentDictionary<Guid, ClientInfo> Clients { get; set; }
    private readonly ILoggerFactory _loggerFactory; 
    private readonly ILogger<Forwarder> _logger;

    public Forwarder(ILogger<Forwarder> logger, ILoggerFactory loggerFactory, PortMap map)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;

        Id = Guid.NewGuid();
        Clients = new ConcurrentDictionary<Guid, ClientInfo>();

        Map = map;
        Server = new TcpListener(map.From);
    }

    public async Task<int> StartServer()
    {
        if (!_started)
        {
            _started = true;
            Server.Start();

            _logger.LogInformation("Server inicializado y en espera...");

            while (true)
            {
                TcpClient cliente = await Server.AcceptTcpClientAsync();

                Guid id = Guid.NewGuid();
                var info = new ClientInfo()
                {
                    Id = id,
                    SourceClient = cliente,
                    DestClient = new TcpClient()
                };
                _logger.LogInformation($"Cliente conectado. {info.ToString()}");

                StatsSingleton.GetInstance().clientsConnected++;

                Clients[id] = info;

                // Manejar la conexión con el cliente de forma asíncrona
                _ = HandleClientAsync(info);
            }
        }

        return 0;
    }

    private async Task HandleClientAsync(ClientInfo info)
    {

        var nextEndpoint = Map.GetNextEndPoint();
        await info.DestClient.ConnectAsync(nextEndpoint);

        CopyStream copyStream = new CopyStream(_loggerFactory.CreateLogger<CopyStream>(),info);
        try
        {
            await copyStream.StartCopyAsync();
        }
        catch (CopyStreamException cex)
        {
            _logger.LogError($"Error CopyStream: {cex.Message}");
        }
        catch (Exception ex) { 
            _logger.LogError($"General error. {ex.Message}");
        }
        finally
        {
            StatsSingleton.GetInstance().clientsConnected++;
            _logger.LogInformation($"Cliente desconectado.{info.ToString()}");
            Clients.Remove(info.Id, out _);

            info.CloseConnections();
        }
    }

    
            // private async Task HandleClientAsync(ClientInfo info)
            // {
            //     NetworkStream stream = info.SourceClient.GetStream();
            //     byte[] buffer = new byte[1024];
            //     int receivedBytes;

            //     try
            //     {
            //         string responseHtml = "<html><body><h1>Welcome to netReverseProxy!</h1><p>Online and ready</p></body></html>";
            //         string response = "HTTP/1.1 200 OK\r\n" +
            //                           "Content-Type: text/html; charset=UTF-8\r\n" +
            //                           $"Content-Length: {Encoding.UTF8.GetByteCount(responseHtml)}\r\n" +
            //                           "Connection: close\r\n" +
            //                           "\r\n" +
            //                           responseHtml;

            //         byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            //         await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

            //         while ((receivedBytes = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            //         {
            //             string mensaje = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            //             _logger.LogTrace($"<- [{receivedBytes}bytes] {mensaje}");

            //             byte[] respuesta = Encoding.UTF8.GetBytes("Mensaje recibido\r\n");
            //             await stream.WriteAsync(respuesta, 0, respuesta.Length);
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError($"Error: {ex.Message}");
            //     }
            //     finally
            //     {
            //         _logger.LogInformation($"Cliente desconectado.{info.ToString()}");
            //         Clients.Remove(info.Id, out _);

            //         stream.Close();
            //         info.SourceClient.Close();

            //     }
            // }

    

}