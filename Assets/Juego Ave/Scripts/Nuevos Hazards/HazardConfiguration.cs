using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject para configurar hazards específicos
/// </summary>
[CreateAssetMenu(fileName = "NewHazardConfig", menuName = "Gameplay/Hazard Configuration")]
public class HazardConfiguration : ScriptableObject
{
    [Header("Hazard Type")]
    public HazardType hazardType;
    public GameObject hazardPrefab;

    [Header("Spawn Settings")]
    public List<SpawnSide> allowedSpawnSides = new List<SpawnSide> { SpawnSide.Top };
    public float spawnChance = 1f; // 0 a 1

    [Header("Pattern Settings (Specific per type)")]
    // Para Acid Rain
    public AcidRainPattern acidRainPattern;

    // Para Toxic Cloud
    public ToxicCloudPattern toxicCloudPattern;

    // Para Wildfire
    public WildfirePattern wildfirePattern;

    [Header("Timing")]
    public float minTimeBetweenSpawns = 5f; // Tiempo minimo entre spawns
    public float maxTimeBetweenSpawns = 10f; // Tiempo maximo entre spawns
}