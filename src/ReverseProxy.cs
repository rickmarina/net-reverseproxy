using System.Net;
using Microsoft.Extensions.Logging;

internal class ReverseProxy {

    private readonly ILogger<ReverseProxy> _logger; 
    private readonly Forwarder _forwarder; 
    private readonly PortMap _portMap; 
    public Dictionary<IPEndPoint, Forwarder> ForwarderMap { get; private set; }
    public ReverseProxy(ILogger<ReverseProxy> logger, Forwarder forwarder, PortMap portMap)
    {
        _logger = logger; 
        _forwarder = forwarder;
        _portMap = portMap;
        ForwarderMap = new();
    }

    public async Task StartForwarder() {
        _logger.LogInformation($"Reverse proxy from {_portMap.From} to {string.Join(",",_portMap.Endpoints.AsEnumerable())}");
        ForwarderMap.Add(_portMap.From, _forwarder);

        await _forwarder.StartServer();

    }
}