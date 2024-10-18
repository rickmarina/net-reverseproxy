using System.Net;
using Microsoft.Extensions.Logging;

internal class PortMap
{
    private int index = 0;
    public IPEndPoint From { get; private set; }
    public IPEndPoint[] Endpoints { get; set; }
    private readonly ILogger<PortMap> _logger;

    public PortMap(ILogger<PortMap> logger, IPEndPoint from, IPEndPoint[] endpoints)
    {
        _logger = logger;
        From = from;
        Endpoints = endpoints;
    }

    public IPEndPoint GetNextEndPoint()
    {
        if (!Endpoints.Any())
            throw new InvalidOperationException("Empty endpoints list.");

         index = (index + 1) % Endpoints.Length; //Round robin

        return Endpoints[index];
    }
}