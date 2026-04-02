using UnityEngine;

public class StaminaBooster : MonoBehaviour
{
    [Tooltip("How much stamina to restore (0–100)")]
    public float restoreAmount = 30f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EnergyBar energy = other.GetComponent<EnergyBar>();

            if (energy != null)
            {
                energy.RestoreEnergy(restoreAmount);
            }

            Destroy(gameObject);
        }
    }
}
