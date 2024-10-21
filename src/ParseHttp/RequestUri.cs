internal class RequestUri
{

    public string? Method { get; set; }
    public string? Url { get; set; }
    public string? HttpVersion { get; set; }

    public RequestUri(string[] lines)
    {
        string[] firstLine = lines[0].Split(' ');

        SetMethod(firstLine[0]);
        SetUrl(firstLine[1]);
        SetHttpVersion(firstLine[2]);
    }

    private void SetMethod(string method) => Method = method.Trim().ToUpper();
    private void SetUrl(string url) => Url = url;
    private void SetHttpVersion(string version) => HttpVersion = version;

    public override string ToString()
    {
        return $"Method {Method} Url {Url} HttpVersion {HttpVersion}";
    }

}