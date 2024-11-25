using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the logic of purchasing player buffs.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerInventory playerInventory;

    public int Balance
    {
        get => playerInventory.money;
        private set
        {
            playerInventory.money = value;
            OnBalanceChanged?.Invoke(Balance);
        }
    }

    // Store current statuses of buffs
    private Buff speedBuff = new Buff(5, 250, BuffType.Speed, 5);
    private Buff damageBuff = new Buff(5, 3, BuffType.Damage);
    private Buff capacityBuff = new Buff(5, 3, BuffType.Capacity);
    public List<Buff> Buffs { get; private set; }

    // Delegates
    public event Action<Buff> OnBuffPurchased;
    public event Action<int> OnBalanceChanged;

    private void Start()
    {
        Buffs = new List<Buff> { speedBuff, damageBuff, capacityBuff };
    }

    public void UpgradeSpeed()
    {
        if (Balance < speedBuff.Cost || speedBuff.IsMaxed) return;

        Balance -= speedBuff.Cost;
        player.IncreaseSpeed(speedBuff.IncreaseAmount);
        speedBuff.Upgrade();
        OnBuffPurchased?.Invoke(speedBuff);

    }

    public void UpgradeDamage()
    {
        if (Balance < damageBuff.Cost) return;

        Balance -= damageBuff.Cost;
        player.IncreaseAttack(damageBuff.IncreaseAmount);
        damageBuff.Upgrade();
        OnBuffPurchased?.Invoke(damageBuff);
    }

    public void UpgradeCapacity()
    {
        if (Balance < capacityBuff.Cost) return;
        Balance -= capacityBuff.Cost;
        playerInventory.IncreaseCapacity(capacityBuff.IncreaseAmount);
        capacityBuff.Upgrade();
        OnBuffPurchased?.Invoke(capacityBuff);
        playerInventory.RestockPizzas();
    }
}

public class Buff
{
    public int Level { get; private set; }
    public int Cost { get; private set; }

    private int MaxLevel { get; } = 0;

    public readonly int IncreaseAmount;

    public readonly BuffType Type;

    public Buff(int initialCost, int incAmount, BuffType buffType, int maxLevel = 0)
    {
        Level = 1;
        Cost = initialCost;
        IncreaseAmount = incAmount;
        Type = buffType;
        MaxLevel = maxLevel;
    }

    public void Upgrade()
    {
        Level += 1;
        Cost = (int)(Cost * 1.33); // was 1.25
    }

    public bool IsMaxed => MaxLevel != 0 && Level == MaxLevel;
}

public enum BuffType
{
    Speed,
    Damage,
    Capacity
}