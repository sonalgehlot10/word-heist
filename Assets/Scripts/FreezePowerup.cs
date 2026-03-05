using UnityEngine;

public class FreezePowerup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerFreezeAbility player = other.GetComponent<PlayerFreezeAbility>();

            if (player != null)
            {
                player.CollectFreeze();
            }

            Destroy(gameObject);
        }
    }
}