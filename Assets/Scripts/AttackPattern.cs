using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public enum MovementType { Linear, Sinusoidal }

[CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Gameplay/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    [Header("Configuración del Proyectil")]
    public GameObject hazardPrefab;
    public float projectileSpeed = 5f;
    public MovementType movementType = MovementType.Linear;
    [Tooltip("Amplitud de la onda para el movimiento sinusoidal.")]
    public float waveAmplitude = 1f;
    [Tooltip("Frecuencia de la onda para el movimiento sinusoidal.")]
    public float waveFrequency = 2f;

    [Header("Configuración del Patrón")]
    [Tooltip("Desde dónde aparecen los proyectiles.")]
    public SpawnDirection spawnDirection = SpawnDirection.Top;
    [Tooltip("Número de proyectiles en una sola ráfaga.")]
    public int projectileCount = 1;
    [Tooltip("Ángulo de la ráfaga (ej. 90 para un abanico frontal).")]
    public float burstAngle = 0f;
    [Tooltip("Tiempo entre cada proyectil de la ráfaga.")]
    public float timeBetweenProjectiles = 0.1f;
}

public enum SpawnDirection { Top, Bottom, Left, Right, Center }