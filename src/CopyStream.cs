using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using static Enums;

internal class CopyStream
{
    private readonly ClientInfo _info;
    private readonly ILogger<CopyStream> _logger;

    public CopyStream(ILogger<CopyStream> logger, ClientInfo info)
    {
        _logger = logger;
        _info = info;
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

    private async Task CopyDataAsync(Stream source, Stream destination, FLOW_STREAM_DIRECTION flowStreamDirection)
    {
        byte[] buffer = new byte[16384]; // Tamaño del buffer de transferencia
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

    private string TransformClientRequest(string receivedContent) { 
        return Regex.Replace(receivedContent, "Host:.*\n", "");
    }
}
