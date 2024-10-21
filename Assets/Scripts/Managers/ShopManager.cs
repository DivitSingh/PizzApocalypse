using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the logic of purchasing player buffs.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PizzaInventar playerInventory;
    
    public int Balance
    {
        get => Player.money;
        private set
        {
            Player.money = value;
            OnBalanceChanged?.Invoke(Balance);
        }
    }
    
    // Store current statuses of buffs
    private Buff healthBuff = new Buff(5, 5, BuffType.Health);
    private Buff damageBuff = new Buff(5, 3, BuffType.Damage);
    private Buff capacityBuff = new Buff(5, 5, BuffType.Capacity);
    public List<Buff> Buffs { get; private set; }

    // Delegates
    public event Action<Buff> OnBuffPurchased;
    public event Action<int> OnBalanceChanged;

    private void Start()
    {
        Buffs = new List<Buff> { healthBuff, damageBuff, capacityBuff };
    }

    public void UpgradeHealth()
    {
        if (Balance < healthBuff.Cost) return;

        Balance -= healthBuff.Cost;
        playerHealth.UpgradeMaxHealth(healthBuff.IncreaseAmount);
        healthBuff.Upgrade();
        OnBuffPurchased?.Invoke(healthBuff);
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
    }
}

// TODO: Should IncreaseAmount change over time or be constant?
public class Buff
{
    public int Level { get; private set; }
    public int Cost { get; private set; }

    public readonly int IncreaseAmount;

    public readonly BuffType Type;

    public Buff(int initialCost, int incAmount, BuffType buffType)
    {
        Level = 1;
        Cost = initialCost;
        IncreaseAmount = incAmount;
        Type = buffType;
    }

    public void Upgrade()
    {
        Level += 1;
        Cost = (int) (Cost * 1.25);
    }
}

public enum BuffType
{
    Health,
    Damage,
    Capacity
}