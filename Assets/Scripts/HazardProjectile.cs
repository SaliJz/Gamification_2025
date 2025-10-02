using Unity.VisualScripting;
using UnityEngine;

public class HazardProjectile : MonoBehaviour
{
    private Vector2 initialDirection;
    private float speed;
    private MovementType moveType;
    private Vector2 perpendicularAxis;
    private float waveAmplitude;
    private float waveFrequency;
    private float timeAlive = 0f;

    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;
    private bool isInitialized = false;

    public void Initialize(Vector2 direction, float projSpeed, MovementType type, float amp = 0, float freq = 0)
    {
        this.initialDirection = direction.normalized;
        this.speed = projSpeed;
        this.moveType = type;
        this.waveAmplitude = amp;
        this.waveFrequency = freq;
        this.perpendicularAxis = new Vector2(-direction.y, direction.x);

        Camera cam = Camera.main;
        float margin = 2f;
        screenBoundsMin = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)) - new Vector3(margin, margin, 0);
        screenBoundsMax = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(margin, margin, 0);
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        timeAlive += Time.deltaTime;
        Vector3 movement = initialDirection * speed * Time.deltaTime;

        if (moveType == MovementType.Sinusoidal)
        {
            float waveOffset = Mathf.Sin(timeAlive * waveFrequency) * waveAmplitude;
            movement += (Vector3)perpendicularAxis * waveOffset * Time.deltaTime;
        }

        transform.Translate(movement);

        if (transform.position.x < screenBoundsMin.x || transform.position.x > screenBoundsMax.x ||
            transform.position.y < screenBoundsMin.y || transform.position.y > screenBoundsMax.y)
        {
            Destroy(gameObject);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (!isInitialized) return;

    //    if (collision.CompareTag("Player"))
    //    {
    //        Destroy(gameObject, 0.25f);
    //    }
    //}
}