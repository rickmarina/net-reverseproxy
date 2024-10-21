internal class HttpRawParser { 
    private readonly string _raw;
    public RequestUri Uri { get; set; }
    public RequestHeader Header { get; set; }
    public RequestBody Body { get; set; }

    private string[] lines;
    
    public HttpRawParser(string raw)
    {
        _raw = raw; 

        lines = raw.Split(new[] { "\\n", "\\r", "\r\n"}, StringSplitOptions.None);

        Uri = new RequestUri(lines); 
        Header = new RequestHeader(lines);
        Body = new RequestBody(lines);
    }

    public override string ToString()
    {
        return $"Uri: {Uri.ToString()} Headers: {Header.ToString()} Body: {Body.ToString()}";
    }




}