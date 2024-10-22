using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using static Enums;

internal class CopyStream
{
    private readonly ClientInfo _info;
    private readonly ILogger<CopyStream> _logger;
    private readonly X509Certificate2 _certificate; 

    public CopyStream(ILogger<CopyStream> logger, ClientInfo info)
    {
        _logger = logger;
        _info = info;

        // Cargar el certificado TLS
        _certificate = new X509Certificate2("certificate/certificate.pfx", "123456");
    }

    public async Task StartCopyAsync()
    {
        // Obtener los flujos de datos para ambos clientes
        using (NetworkStream stream1 = _info.SourceClient.GetStream())
        using (NetworkStream stream2 = _info.DestClient.GetStream())
        {
            // Copiar datos en ambas direcciones de forma simultánea
            Task task1 = CopyDataAsync(stream1, stream2, FLOW_STREAM_DIRECTION.CLIENT);
            Task task2 = CopyDataAsync(stream2, stream1, FLOW_STREAM_DIRECTION.SERVER);

            try {
                // Esperar a que ambas tareas finalicen
                await Task.WhenAll(task1, task2);
            } catch (Exception ex) { 
                _logger.LogError($"{ex.Message}");
            } finally {
                // Cerrar las conexiones cuando una de las tareas finalice
                _logger.LogInformation($"Closed links with {_info}");
                _info.CloseConnections();
            }

        }
    }

    public async Task StartCopyAsyncSSL() { 
        using NetworkStream clientStream = _info.SourceClient.GetStream();
        using SslStream clientSslStream = new SslStream(clientStream, false,new RemoteCertificateValidationCallback(ValidateClientCertificate), null);

        _logger.LogInformation($"Autenticamos el cliente contra el proxy, le mandamos el certificado");
        // Autenticar el servidor (proxy) ante el cliente usando el certificado
        await clientSslStream.AuthenticateAsServerAsync(_certificate, false, false);

        using NetworkStream serverStream = _info.DestClient.GetStream();
        using SslStream serverSslStream = new SslStream(serverStream,false);

        // Autenticar el cliente (proxy) ante el backend
        _logger.LogInformation($"Autenticamos el proxy contra el servidor con el host {_info.DestclientHost}");
        await serverSslStream.AuthenticateAsClientAsync(_info.DestclientHost);

        // Copiar datos en ambas direcciones de forma simultánea
        Task task1 = CopyDataAsync(clientSslStream, serverSslStream, FLOW_STREAM_DIRECTION.CLIENT);
        Task task2 = CopyDataAsync(serverSslStream, clientSslStream, FLOW_STREAM_DIRECTION.SERVER);

        try {
            // Esperar a que ambas tareas finalicen
            await Task.WhenAll(task1, task2);
        } catch (Exception ex) { 
            _logger.LogError($"{ex.Message}");
        } finally {
            // Cerrar las conexiones cuando una de las tareas finalice
            _logger.LogInformation($"Closed links with {_info}");
            _info.CloseConnections();
        }

    }

    private async Task CopyDataAsync(Stream source, Stream destination, FLOW_STREAM_DIRECTION flowStreamDirection)
    {
        byte[] buffer = new byte[8192]; // Tamaño del buffer de transferencia
        int bytesRead;

        try
        {
            // Leer y escribir datos hasta que no haya más
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var receivedContent = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine($"<- {flowStreamDirection} {receivedContent}");

                if (flowStreamDirection == FLOW_STREAM_DIRECTION.CLIENT)
                    StatsSingleton.GetInstance().bytesReceivedFromClients += bytesRead;
                else 
                    StatsSingleton.GetInstance().bytesReceivedFromServers += bytesRead;

                //Realizar manipulación del stream del cliente 
                string toSend = receivedContent;
                if (flowStreamDirection == FLOW_STREAM_DIRECTION.CLIENT) {
                    toSend = TransformClientRequest(receivedContent); 
                    Console.WriteLine($"-> {flowStreamDirection} {toSend}");
                } 

                var toSendBytes = Encoding.UTF8.GetBytes(toSend); 

                await destination.WriteAsync(toSendBytes, 0, toSendBytes.Length);
                await destination.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            throw new CopyStreamException("Copystream aborted", ex);
        }
    }

    private async Task CopyDataAsyncSsl(SslStream source, SslStream destination, FLOW_STREAM_DIRECTION flowStreamDirection)
    {
        byte[] buffer = new byte[8192]; // Tamaño del buffer de transferencia
        int bytesRead;

        try
        {
            // Leer y escribir datos hasta que no haya más
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var receivedContent = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine($"<- {flowStreamDirection} {receivedContent.Substring(0,1500)}");

                //Realizar manipulación del stream del cliente 
                // string toSend = receivedContent;
                // if (flowStreamDirection == FLOW_STREAM_DIRECTION.CLIENT) {
                //     toSend = TransformClientRequest(receivedContent); 
                //     Console.WriteLine($"-> {flowStreamDirection} {toSend}");
                // } 

                // var toSendBytes = Encoding.UTF8.GetBytes(toSend); 

                await destination.WriteAsync(buffer, 0, bytesRead);
                await destination.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            throw new CopyStreamException("Copystream ssl aborted", ex);
        }
    }
    private bool ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true; // Aceptar cualquier certificado de cliente (puede ser ajustado según las necesidades)
    }
    private string TransformClientRequest(string clientContentMessage) { 
        clientContentMessage = Regex.Replace(clientContentMessage, "Host:.*\n", "Host: www.google.com\n");
        // clientContentMessage = Regex.Replace(clientContentMessage, "Accept-Encoding:*\n","Accept-Encoding: gzip\n");

        return clientContentMessage;
    }
}
