using System.Net.Sockets;

internal static class TcpClientExtensions {

    public static bool IsSocketConnected(this TcpClient tcpClient) {
        try {
            if (tcpClient.Client is null) 
                return false; 
            
            return !(tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0);
        } catch (SocketException) {
            return false;
        }
    }

}