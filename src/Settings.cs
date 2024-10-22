using System.Net;

public class ReverseProxySettings
{
    public required HostSetting listen { get; set; }
    public required List<HostSetting> forwards { get; set; }
}

public class Settings
{
    public double version { get; set; }
    public required ReverseProxySettings config { get; set; }
}

public class HostSetting {
    public required string name { get; set; }
    public required string host { get; set; }
    public required string ip { get; set; }
    public required int port { get; set; }
    public required bool ssl { get; set; }

    public IPEndPoint GetIPEndpoint() => new IPEndPoint(IPAddress.Parse(ip), port);

}