using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles the scaling of round configuration values as the game progresses.
/// </summary>
public class RoundProgression
{
    private int round = 0;
    public RoundConfiguration Configuration { get; private set; }

    private const int OrderSizeCap = 4;
    private const float PatienceCap = 8f; // old was 5f

    public RoundProgression()
    {
        Next();
    }

    public void Next()
    {
        round++;
        var customerConfig = CreateCustomerConfiguration();
        var spawnConfig = CreateSpawnConfiguration(round);
        
        var duration = GetDuration(round);
        var passScore = GetPassScore(round);
        Configuration = new RoundConfiguration(passScore, duration, spawnConfig, customerConfig);
    }

    private static int GetDuration(int round)
    {
        int durationInSeconds = round switch
        {
            1 => 45,
            2 or 3 => 60,
            4 or 5 => 90,
            6 or 7 => 105,
            _ => 120 // For round 8 and beyond
        };
        return durationInSeconds;
    }

    // Old GetDuration below
    // private static int GetDuration(GenerateCustomerConfiguration customerConfig, SpawnConfiguration spawnConfig)
    // {
    //     var duration = 1 + (int) ((spawnConfig.Count - 1) * spawnConfig.Interval + (customerConfig.Patience * 1.2));
    //     duration = RoundUp(duration);
    //     return duration;
    // }

    private int GetPassScore(int round)
    {
        // passScore equals round count, expect after round 10.
        if (round > 9)
        {
            return 10;
        }
        return round;
    }

    //  old GetPassScore below
    // private int GetPassScore(int numCustomers)
    // {
    //     if (round == 1) return 1;
    //     return (int) (0.5 * numCustomers);
    // }

    private int TotalCustomerCount(int round)
    {
        int totalCustomerCount = (int)(GetDuration(round) / SpawnInterval) + 1;
        return totalCustomerCount;
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
        <= 1 => 25f,
        2 => 22f,
        3 => 20f,
        4 => 20f,
        5 => 17f,
        6 => 15f,
        7 => 13f,
        8 => 12f,
        9 => 10f,
        >= 10 => PatienceCap, //10f
        // old values below
        // <= 1 => 10f,
        // 2 => 9.25f,
        // 3 => 8.75f,
        // < 5 => 8f,
        // < 7 => 7f,
        // < 10 => 6f,
        // >= 10 => PatienceCap
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
        <= 3 => 2,
        <= 6 => 3,
        >= 7 => OrderSizeCap,
    };

    #endregion

    #region Spawn Configuration
    private SpawnConfiguration CreateSpawnConfiguration(int round)
    {
        return new SpawnConfiguration(TotalCustomerCount(round), CustomerPerRound, SpawnInterval);
    }
    // old SpawnConfiguration below
    // private SpawnConfiguration CreateSpawnConfiguration(float patience, int numSpawnPoints)
    // {
    //     return new SpawnConfiguration(Count, SpawnInterval(patience, numSpawnPoints));
    // }

    private float SpawnInterval => round switch
    {
        <= 1 => 25f,
        2 => 22f,
        3 => 20f,
        4 => 20f,
        5 => 17f,
        6 => 15f,
        7 => 13f,
        8 => 12f,
        9 => 10f,
        >= 10 => 8f,
    };
    // old SpawnInterval below
    // private float SpawnInterval(float patience, int numSpawnPoints)
    // {
    //     var intervalCap = patience / numSpawnPoints; // Can't spawn faster than patience runs out for initial spawn
    //     var interval = round switch
    //     {
    //         // old values 
    //         // <= 3 => 4.5f * intervalCap,
    //         // <= 6 => 3.5f * intervalCap,
    //         // _ => 2.75f * intervalCap
    //     };
    //     return interval;
    // } 


    
    private int CustomerPerRound => round switch
    {
        <= 1 => 1,
        <= 3 => 2,
        <= 6 => 3,
        >= 7 => 4 // For round 8 and beyond 4 customers max
    };
    // old CustomerCount below
    // private int CustomerCount => round switch
    // {
    //     <= 3 => 3 + round,
    //     > 3 => 2 * round
    // };
    #endregion

}

