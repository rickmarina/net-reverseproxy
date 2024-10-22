using System.Net;
using Microsoft.Extensions.Logging;

internal class PortMap
{
    private int index = 0;
    public HostSetting From { get; private set; }
    public List<HostSetting> Endpoints { get; set; }
    private readonly ILogger<PortMap> _logger;

    public PortMap(ILogger<PortMap> logger, HostSetting from, List<HostSetting> endpoints)
    {
        _logger = logger;
        From = from;
        Endpoints = endpoints;
    }

    public HostSetting GetNextEndPoint()
    {
        if (!Endpoints.Any())
            throw new InvalidOperationException("Empty endpoints list.");

         index = (index + 1) % Endpoints.Count; //Round robin

        return Endpoints[index];
    }
}