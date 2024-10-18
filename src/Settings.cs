public class ReverseSettings
{
    public required ListenSettings listen { get; set; }
    public required List<string> forwards { get; set; }
}

public class Settings
{
    public double version { get; set; }
    public required ReverseSettings config { get; set; }
}

public class ListenSettings {
    public required string ip { get; set; }
    public required int port { get; set; }

}