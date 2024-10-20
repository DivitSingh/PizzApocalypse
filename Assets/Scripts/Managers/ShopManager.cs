using UnityEngine;

/// <summary>
/// Manages the logic of purchasing player buffs.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PizzaInventar playerInventory;
    
    // TODO: Replace with player balance
    private int Balance { get; } = 1_000_000;
    
    // Store current statuses of buffs
    private Buff healthBuff = new Buff(200, 10);
    private Buff damageBuff = new Buff(200, 5);
    private Buff capacityBuff = new Buff(200, 5);
    
    // TODO: Add return indicating updated level and cost
    public void UpgradeHealth()
    {
        if (Balance < healthBuff.Cost) return;
        healthBuff.Upgrade();
        playerHealth.UpgradeMaxHealth(healthBuff.IncreaseAmount);
    }

    public void UpgradeDamage()
    {
        if (Balance < damageBuff.Cost) return;
        damageBuff.Upgrade();
        // TODO: Increase player health somewhere
    }
    
    public void UpgradeCapacity()
    {
        if (Balance < capacityBuff.Cost) return;
        capacityBuff.Upgrade();
        playerInventory.IncreaseCapacity(capacityBuff.IncreaseAmount);
    }
    
    // TODO: Implement UI
}

public class Buff
{
    public int Level { get; private set; }
    public int Cost { get; private set; }

    public readonly int IncreaseAmount;

    public Buff(int cost, int incAmount)
    {
        Level = 1;
        Cost = cost;
        IncreaseAmount = incAmount;
    }

    public void Upgrade()
    {
        Level += 1;
        Cost = (int) (Cost * 1.25);
    }
}