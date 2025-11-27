using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public enum MovementType { Linear, Sinusoidal }
public enum PatternType { Burst, Stream }
public enum SpawnOrigin { Top, Bottom, Left, Right, Center, AtPlayerLocation }

[CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Gameplay/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    [Header("Configuración General")]
    [Tooltip("Prefab del proyectil a disparar.")]
    public GameObject hazardPrefab;
    [Tooltip("Define si el ataque es una ráfaga instantánea (Burst) o un chorro continuo (Stream).")]
    public PatternType patternType = PatternType.Burst;

    [Header("Origen y Dirección del Patrón")]
    [Tooltip("Desde dónde en la pantalla se origina el ataque.")]
    public SpawnOrigin spawnOrigin = SpawnOrigin.Top;
    [Tooltip("Dirección principal del ataque (0=Derecha, 90=Arriba, 180=Izquierda, 270=Abajo).")]
    public float baseAngle = 270f; // Por defecto, dispara hacia abajo.
    [Tooltip("Si está marcado, 'baseAngle' se ignora y el ataque apunta a la posición del jugador al iniciar.")]
    public bool aimAtPlayer = false;

    [Header("Comportamiento Individual del Proyectil")]
    public float projectileSpeed = 8f;
    public MovementType movementType = MovementType.Linear;
    [Tooltip("Parámetros para el movimiento Sinusoidal.")]
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;

    [Header("Configuración de Ráfaga (Burst)")]
    [Tooltip("Número de proyectiles en la ráfaga. Para un solo disparo, usa 1.")]
    public int projectileCount = 8;
    [Tooltip("El ángulo total del abanico de proyectiles. Usar 360 para un círculo completo (Ring).")]
    [Range(0, 360)]
    public float spreadAngle = 360f;

    [Header("Configuración de Chorro (Stream)")]
    [Tooltip("Duración total del patrón en segundos.")]
    public float duration = 3f;
    [Tooltip("Proyectiles disparados por segundo.")]
    public float fireRate = 5f;
    [Tooltip("Velocidad a la que el ángulo del ataque rota durante el Stream (grados por segundo), para crear espirales.")]
    public float spinSpeed = 0f;
}