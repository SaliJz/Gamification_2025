using UnityEngine;

public class SlowArea : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float slowFactor = 0.5f;
    public int damageOnEnter = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerMovement = other.GetComponent<PlayerController>();

            if (playerMovement != null)
            {
                playerMovement.ApplySlow(slowFactor);
                Debug.Log("Jugador ha entrado en la zona lenta. Velocidad reducida.");
                playerMovement.TakeDamage(damageOnEnter);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerMovement = other.GetComponent<PlayerController>();

            if (playerMovement != null)
            {
                playerMovement.RemoveSlow();
                Debug.Log("Jugador ha salido de la zona lenta. Velocidad restaurada.");
            }
        }
    }
}
