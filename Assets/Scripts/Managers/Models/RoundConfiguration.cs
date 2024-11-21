public class RoundConfiguration
{
    public int PassScore { get; }
    public int Duration { get; }
    public SpawnConfiguration SpawnConfiguration { get; }
    public GenerateCustomerConfiguration CustomerConfiguration { get; }
    
    public RoundConfiguration(int passScore, int duration, SpawnConfiguration spawnConfiguration, GenerateCustomerConfiguration customerConfiguration)
    {
        PassScore = passScore;
        Duration = duration;
        SpawnConfiguration = spawnConfiguration;
        CustomerConfiguration = customerConfiguration;
    }
}

public class SpawnConfiguration
{
    public int TotalCustomerCount { get; } 
    public int CustomerPerWave { get; }
    public float Interval { get; }

    public SpawnConfiguration(int totalCustomerCount, int customerPerWave, float interval)
    {
        TotalCustomerCount = totalCustomerCount;
        CustomerPerWave = customerPerWave;
        Interval = interval;
    }
}

/// <summary>
/// A representation of the values used to spawn a customer during a given round.
/// <remarks>This is not used to instantiate an individual customer, but rather to define preset values.</remarks>
/// </summary>
public class GenerateCustomerConfiguration
{
    public float Patience { get; }
    public float Satisfaction { get; }
    public float Attack { get; }
    public int MaxOrderSize { get; }
    
    public GenerateCustomerConfiguration(float patience, float satisfaction, float attack, int maxOrderSize)
    {
        Patience = patience;
        Satisfaction = satisfaction;
        Attack = attack;
        MaxOrderSize = maxOrderSize;
    }
}