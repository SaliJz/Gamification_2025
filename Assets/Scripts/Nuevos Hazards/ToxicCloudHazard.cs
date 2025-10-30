using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hazard de nube tóxica. Se mueve por la pantalla en diferentes patrones.
/// </summary>
public class ToxicCloudHazard : BaseHazard
{
    [Header("Toxic Cloud Configuration")]
    [SerializeField] private ToxicCloudPattern pattern = ToxicCloudPattern.LinearDrift;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 1f;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private bool continuousDamage = true;
    [SerializeField] private float damageInterval = 0.5f;

    private Vector2 moveDirection;
    private Vector2 initialPosition;
    private Vector2 orbitCenter;
    private float movementTime = 0f;
    private float lastDamageTime = 0f;
    private HashSet<Collider2D> collidingPlayers = new HashSet<Collider2D>();

    public override void Initialize(Vector2 position, SpawnSide spawnSide, Transform player)
    {
        base.Initialize(position, spawnSide, player);
        initialPosition = position;
        SetupMovementDirection(spawnSide);

        // Para órbita circular, establecer centro
        if (pattern == ToxicCloudPattern.CircularOrbit)
        {
            orbitCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
    }

    private void SetupMovementDirection(SpawnSide side)
    {
        switch (side)
        {
            case SpawnSide.Top:
                moveDirection = Vector2.down;
                break;
            case SpawnSide.Bottom:
                moveDirection = Vector2.up;
                break;
            case SpawnSide.Left:
                moveDirection = Vector2.right;
                break;
            case SpawnSide.Right:
                moveDirection = Vector2.left;
                break;
        }
    }

    protected override void OnHazardActivated()
    {
        movementTime = 0f;
    }

    protected override void UpdateHazardBehavior()
    {
        movementTime += Time.deltaTime;

        switch (pattern)
        {
            case ToxicCloudPattern.LinearDrift:
                UpdateLinearMovement();
                break;
            case ToxicCloudPattern.ZigzagSweep:
                UpdateZigzagMovement();
                break;
            case ToxicCloudPattern.CircularOrbit:
                UpdateCircularMovement();
                break;
        }

        // Daño continuo
        if (continuousDamage && Time.time >= lastDamageTime + damageInterval)
        {
            ApplyContinuousDamage();
            lastDamageTime = Time.time;
        }
    }

    private void UpdateLinearMovement()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void UpdateZigzagMovement()
    {
        Vector2 perpendicular = new Vector2(-moveDirection.y, moveDirection.x);
        float zigzag = Mathf.Sin(movementTime * zigzagFrequency * Mathf.PI * 2) * zigzagAmplitude;

        Vector2 movement = moveDirection * moveSpeed * Time.deltaTime;
        movement += perpendicular * zigzag * Time.deltaTime;

        transform.position += (Vector3)movement;
    }

    private void UpdateCircularMovement()
    {
        float angle = movementTime * orbitSpeed;
        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * orbitRadius,
            Mathf.Sin(angle) * orbitRadius
        );

        transform.position = orbitCenter + offset;
    }

    private void ApplyContinuousDamage()
    {
        foreach (var player in collidingPlayers)
        {
            if (player != null)
            {
                OnPlayerHit(player);
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            collidingPlayers.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collidingPlayers.Remove(collision);
        }
    }

    protected override void OnHazardImpact()
    {
        // No se destruye al impactar, es una nube persistente
    }
}