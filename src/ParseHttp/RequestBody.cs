internal class RequestBody {

    public string Body { get; set; } = "";
    public RequestBody(string[] lines)
    {
        int breakLine = Array.IndexOf(lines, "");

        if ((breakLine > -1) && (breakLine < lines.Length-1)) {
            Body = lines[breakLine+1];
        }
    }

    public override string ToString() => $"{Body}";

}