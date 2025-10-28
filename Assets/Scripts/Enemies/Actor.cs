using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Legacy Health System (Use Health Component Instead)")]
    [Tooltip("Only used if no Health component is found")]
    public int maxHealth = 50;
    private int currentHealth;
    
    // New Health System
    private Health healthComponent;
    private bool usingHealthComponent = false;

    void Awake()
    {
        // Try to find Health component first
        healthComponent = GetComponent<Health>();
        
        if (healthComponent != null)
        {
            usingHealthComponent = true;
            // Subscribe to health component events
            healthComponent.OnDeath += Death;
            Debug.Log($"[{gameObject.name}] Using new Health component system");
        }
        else
        {
            // Fall back to legacy system
            usingHealthComponent = false;
            currentHealth = maxHealth;
            Debug.Log($"[{gameObject.name}] Using legacy health system - Health: {currentHealth}/{maxHealth}");
        }
    }

    public void TakeDamage(int amount)
    {
        if (usingHealthComponent && healthComponent != null)
        {
            // Use new Health component
            healthComponent.TakeDamage(amount);
        }
        else
        {
            // Use legacy system
            currentHealth -= amount;
            Debug.Log($"[{gameObject.name}] Legacy: Took {amount} damage - Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Death();
            }
        }
    }
    
    public void Heal(int amount)
    {
        if (usingHealthComponent && healthComponent != null)
        {
            healthComponent.Heal(amount);
        }
        else
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"[{gameObject.name}] Legacy: Healed {amount} - Health: {currentHealth}/{maxHealth}");
        }
    }
    
    public int GetCurrentHealth()
    {
        if (usingHealthComponent && healthComponent != null)
        {
            return healthComponent.CurrentHealth;
        }
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        if (usingHealthComponent && healthComponent != null)
        {
            return healthComponent.MaxHealth;
        }
        return maxHealth;
    }
    
    public bool IsDead()
    {
        if (usingHealthComponent && healthComponent != null)
        {
            return healthComponent.IsDead;
        }
        return currentHealth <= 0;
    }

    void Death()
    {
        Debug.Log($"[{gameObject.name}] DIED");
        
        // Death function
        // TEMPORARY: Destroy Object
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= Death;
        }
    }
}
