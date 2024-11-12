using UnityEngine;

/// <summary>
/// Handles the scaling of round configuration values as the game progresses.
/// </summary>
public class RoundProgression
{
    private int round = 0;
    public RoundConfiguration Configuration { get; private set; }

    private const int OrderSizeCap = 4;
    private const float PatienceCap = 10f; // old was 5f

    public RoundProgression()
    {
        Next();
    }

    public void Next()
    {
        round++;
        var customerConfig = CreateCustomerConfiguration();
        var spawnConfig = CreateSpawnConfiguration(customerConfig.Patience, 5);
        
        var duration = GetDuration(customerConfig, spawnConfig);
        var passScore = GetPassScore(spawnConfig.Count);
        Configuration = new RoundConfiguration(passScore, duration, spawnConfig, customerConfig);
    }

    private static int GetDuration(GenerateCustomerConfiguration customerConfig, SpawnConfiguration spawnConfig)
    {
        var duration = 1 + (int) ((spawnConfig.Count - 1) * spawnConfig.Interval + (customerConfig.Patience * 1.2));
        duration = RoundUp(duration);
        return duration;
    }

    private int GetPassScore(int numCustomers)
    {
        if (round == 1) return 1;
        return (int) (0.5 * numCustomers);
    }
    
    /// <summary>
    /// Rounds the given number up to the nearest multiple of 5 that is >= num.
    /// </summary>
    /// <param name="num">Number to round</param>
    /// <returns></returns>
    private static int RoundUp(int num)
    {
        if (num % 5 == 0) return num;
        return (10 - num % 10) + num;
    }
    
    #region Customer Configuration
    private GenerateCustomerConfiguration CreateCustomerConfiguration()
    {
        return new GenerateCustomerConfiguration(Patience, Satisfaction, Attack, OrderSize);
    }


    // private float Patience => round switch
    // {
    //     <= 1 => 25f,
    //     2 => 24f,
    //     3 => 23f,
    //     < 5 => 20f,
    //     < 7 => 17.5f,
    //     < 10 => 15f,
    //     >= 10 => PatienceCap
    // };

    // Old config: below   
    private float Patience => round switch
    {
        <= 1 => 10f,
        2 => 9.25f,
        3 => 8.75f,
        < 5 => 8f,
        < 7 => 7f,
        < 10 => 6f,
        >= 10 => PatienceCap
    };

    private const float BaseSatisfaction = 60f;
    private const float SatisfactionIncrease = 10f;
    private float Satisfaction => 60f + (round - 1) * SatisfactionIncrease;

    private const float BaseAttack = 15f;
    private const float AttackIncrease = 5f;
    private float Attack => BaseAttack + (round / 2) * AttackIncrease;  // Increase every 2 rounds
    
    private int OrderSize => round switch
    {
        <= 1 => 1,
        < 4 => 2,
        < 6 => 3,
        >= 6 => OrderSizeCap,
    };

    #endregion

    #region Spawn Configuration
    private SpawnConfiguration CreateSpawnConfiguration(float patience, int numSpawnPoints)
    {
        return new SpawnConfiguration(CustomerCount, SpawnInterval(patience, numSpawnPoints));
    }

    private float SpawnInterval(float patience, int numSpawnPoints)
    {
        var intervalCap = patience / numSpawnPoints; // Can't spawn faster than patience runs out for initial spawn
        var interval = round switch
        {
            // <= 3 => 5f * intervalCap,
            // <= 6 => 4f * intervalCap,
            // _ => 3.5f * intervalCap
            // old values below
            <= 3 => 4.5f * intervalCap,
            <= 6 => 3.5f * intervalCap,
            _ => 2.75f * intervalCap
        };
        return interval;
    }
    
    private int CustomerCount => round switch
    {
        <= 3 => 3 + round,
        > 3 => 2 * round
    };
    #endregion

}

