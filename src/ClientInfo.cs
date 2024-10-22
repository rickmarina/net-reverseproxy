using System.Net.Sockets;

internal class ClientInfo
{
    public Guid Id { get; set; }
    public required TcpClient DestClient { get; set; }
    public required bool DestClientSSL {get;set;}
    public required string DestclientHost {get;set;}
    public required TcpClient SourceClient { get; set; }

    public override string ToString()
    {
        return $"Client ID: {Id}. Socket connected: {SourceClient?.IsSocketConnected()}";
    }

    public void CloseConnections() {
        if (SourceClient.Connected) {
            SourceClient.Close();
            SourceClient.Dispose();
        }
        if (DestClient.Connected) {
            DestClient.Close();
            DestClient.Dispose();
        }
    }

}