using UnityEngine;

public class SlowArea : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float slowFactor = 0.5f; // Factor de reducción de velocidad (0.5 = 50% de velocidad)
    public int damageOnEnter = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                Debug.Log("Jugador ha entrado en la zona lenta. Velocidad reducida.");
                playerController.ApplySlow(slowFactor);
                playerController.TakeDamage(damageOnEnter);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                Debug.Log("Jugador ha salido de la zona lenta. Velocidad restaurada.");
                playerController.RemoveSlow();
            }
        }
    }
}