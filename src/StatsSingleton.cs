internal sealed class StatsSingleton {

    private static readonly Lazy<StatsSingleton> _instance = new Lazy<StatsSingleton>(
        () => new StatsSingleton(), LazyThreadSafetyMode.ExecutionAndPublication
    );

    public static StatsSingleton GetInstance() { 
        return _instance.Value;
    }

    private StatsSingleton() { 

    }
    
    public int clientsConnected { get; set; }
    public long bytesReceivedFromClients {get;set;}
    public long bytesReceivedFromServers {get;set;}
}