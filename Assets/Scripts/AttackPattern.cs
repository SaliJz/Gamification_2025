using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public enum MovementType { Linear, Sinusoidal }
public enum PatternType { Burst, Stream }
public enum SpawnOrigin { Top, Bottom, Left, Right, Center, AtPlayerLocation }

[CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Gameplay/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    [Header("Configuraci�n General")]
    [Tooltip("Prefab del proyectil a disparar.")]
    public GameObject hazardPrefab;
    [Tooltip("Define si el ataque es una r�faga instant�nea (Burst) o un chorro continuo (Stream).")]
    public PatternType patternType = PatternType.Burst;

    [Header("Origen y Direcci�n del Patr�n")]
    [Tooltip("Desde d�nde en la pantalla se origina el ataque.")]
    public SpawnOrigin spawnOrigin = SpawnOrigin.Top;
    [Tooltip("Direcci�n principal del ataque (0=Derecha, 90=Arriba, 180=Izquierda, 270=Abajo).")]
    public float baseAngle = 270f; // Por defecto, dispara hacia abajo.
    [Tooltip("Si est� marcado, 'baseAngle' se ignora y el ataque apunta a la posici�n del jugador al iniciar.")]
    public bool aimAtPlayer = false;

    [Header("Comportamiento Individual del Proyectil")]
    public float projectileSpeed = 8f;
    public MovementType movementType = MovementType.Linear;
    [Tooltip("Par�metros para el movimiento Sinusoidal.")]
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;

    [Header("Configuraci�n de R�faga (Burst)")]
    [Tooltip("N�mero de proyectiles en la r�faga. Para un solo disparo, usa 1.")]
    public int projectileCount = 8;
    [Tooltip("El �ngulo total del abanico de proyectiles. Usar 360 para un c�rculo completo (Ring).")]
    [Range(0, 360)]
    public float spreadAngle = 360f;

    [Header("Configuraci�n de Chorro (Stream)")]
    [Tooltip("Duraci�n total del patr�n en segundos.")]
    public float duration = 3f;
    [Tooltip("Proyectiles disparados por segundo.")]
    public float fireRate = 5f;
    [Tooltip("Velocidad a la que el �ngulo del ataque rota durante el Stream (grados por segundo), para crear espirales.")]
    public float spinSpeed = 0f;
}