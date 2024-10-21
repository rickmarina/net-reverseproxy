internal class HttpRawParser { 
    private readonly string _raw;
    public RequestUri RequestUri { get; set; }
    public RequestHeader RequestHeader { get; set; }
    public RequestBody RequestBody { get; set; }

    private string[] lines;
    
    public HttpRawParser(string raw)
    {
        _raw = raw; 

        lines = raw.Split(["\\n", "\\r", "\r\n"], StringSplitOptions.None);

        RequestUri = new RequestUri(lines); 
        RequestHeader = new RequestHeader(lines);
        RequestBody = new RequestBody(lines);
    }

    public override string ToString()
    {
        return $"Uri: {RequestUri.ToString()} Headers: {RequestHeader.ToString()} Body: {RequestBody.ToString()}";
    }




}