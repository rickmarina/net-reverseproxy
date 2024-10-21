using Microsoft.Extensions.Primitives;

internal class RequestHeader {
    public Dictionary<string,string> Headers { get; set; }
    public RequestHeader(string[] lines)
    {
        Headers = new(); 

        int breakLine = Array.IndexOf(lines, "");
        for (int i=1; i< breakLine-1; i++) { 
            var infoHeader = lines[i].Split(":").ToArray(); 
            Headers.Add(infoHeader[0].Trim(), infoHeader[1].Trim());
        }
    }

    public override string ToString()
    {
        return string.Join(",", Headers.Select(x=> $"{x.Key}:{x.Value}"));
    }
}