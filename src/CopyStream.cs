using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

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
            Task task1 = CopyDataAsync(stream1, stream2);
            Task task2 = CopyDataAsync(stream2, stream1);

            try {
                // Esperar a que ambas tareas finalicen
                await Task.WhenAll(task1, task2);
            } catch (Exception ex) { 
                _logger.LogError($"error startcopyasync: {ex.Message}");
            } finally {
                // Cerrar las conexiones cuando una de las tareas finalice
                _logger.LogInformation($"cerramos las conexiones source/dest de info");
                _info.CloseConnections();
            }

        }
    }

    private async Task CopyDataAsync(Stream source, Stream destination)
    {
        byte[] buffer = new byte[8192]; // Tamaño del buffer de transferencia
        int bytesRead;

        try
        {
            // Leer y escribir datos hasta que no haya más
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var receivedContent = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine($"<- {receivedContent}");

                await destination.WriteAsync(buffer, 0, bytesRead);
                await destination.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            throw new CopyStreamException("Copystream aborted", ex);
        }
    }
}
