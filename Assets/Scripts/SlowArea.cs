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
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.ApplySlow(slowFactor);
                Debug.Log("Jugador ha entrado en la zona lenta. Velocidad reducida.");
                playerController.TakeDamage(damageOnEnter);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.RemoveSlow();
                Debug.Log("Jugador ha salido de la zona lenta. Velocidad restaurada.");
            }
        }
    }
}