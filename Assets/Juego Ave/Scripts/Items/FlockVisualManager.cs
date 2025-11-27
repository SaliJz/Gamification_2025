using System.Collections.Generic;
using UnityEngine;

public class FlockVisualManager : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("El prefab visual del ave (solo sprite, sin lógica)")]
    [SerializeField] private GameObject flockBirdVisualPrefab;
    [SerializeField] private float orbitRadius = 1.0f;
    [SerializeField] private float orbitSpeed = 100f;

    private List<GameObject> visualFlock = new List<GameObject>();
    private float currentAngle = 0f;

    // Posiciones orbitales diagonales (4 posiciones)
    private readonly Vector2[] orbitPositions = new Vector2[]
    {
        new Vector2(1, 1).normalized,   // Arriba-Derecha
        new Vector2(-1, 1).normalized,  // Arriba-Izquierda
        new Vector2(-1, -1).normalized, // Abajo-Izquierda
        new Vector2(1, -1).normalized   // Abajo-Derecha
    };

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Solo actualizar si se está jugando
        GameState state = GameManager.Instance.CurrentState;
        if (state != GameState.Playing && state != GameState.Tutorial)
        {
            return;
        }

        UpdateFlockSize();
        UpdateFlockOrbit();
    }

    /// <summary>
    /// Compara el tamaño de la bandada del GameManager con la visual y la ajusta.
    /// </summary>
    private void UpdateFlockSize()
    {
        // El FlockSize del GameManager incluye al jugador (tamaño 1 = solo jugador).
        // El número de aves visuales es FlockSize - 1.
        int targetVisualCount = GameManager.Instance.FlockSize - 1;

        // Añadir aves si faltan
        while (visualFlock.Count < targetVisualCount)
        {
            if (flockBirdVisualPrefab == null) break;

            // Instanciar el ave visual
            GameObject birdVisual = Instantiate(flockBirdVisualPrefab, transform.position, Quaternion.identity, transform);
            visualFlock.Add(birdVisual);
        }

        // Quitar aves si sobran
        while (visualFlock.Count > targetVisualCount)
        {
            int lastIndex = visualFlock.Count - 1;
            GameObject birdToRemove = visualFlock[lastIndex];
            visualFlock.RemoveAt(lastIndex);
            Destroy(birdToRemove);
        }
    }

    /// <summary>
    /// Actualiza la posición orbital de las aves visuales.
    /// </summary>
    private void UpdateFlockOrbit()
    {
        if (visualFlock.Count == 0) return;

        // Actualizar el ángulo de órbita
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;

        float angleRad = currentAngle * Mathf.Deg2Rad;

        // Reposicionar cada ave
        for (int i = 0; i < visualFlock.Count; i++)
        {
            if (visualFlock[i] == null) continue;

            // Obtener la posición base (diagonal)
            Vector2 basePos = orbitPositions[i % orbitPositions.Length];

            // Rotar esa posición base
            float x = basePos.x * Mathf.Cos(angleRad) - basePos.y * Mathf.Sin(angleRad);
            float y = basePos.x * Mathf.Sin(angleRad) + basePos.y * Mathf.Cos(angleRad);

            Vector3 orbitPos = new Vector3(x, y, 0) * orbitRadius;

            // Posicionar relativo al jugador
            visualFlock[i].transform.position = transform.position + orbitPos;
        }
    }
}
