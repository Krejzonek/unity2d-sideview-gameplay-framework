
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponHitbox : MonoBehaviour
{
    private Player player;
    private BoxCollider2D bladeCollider;
    private readonly HashSet<EnemyAI> hitEnemies = new HashSet<EnemyAI>();
    private bool isAttacking;

    private void Awake()
    {
        bladeCollider = GetComponent<BoxCollider2D>();
        bladeCollider.isTrigger = true;
        bladeCollider.enabled = false;
    }

    public void Initialize(Player owner)
    {
        player = owner;
    }

    public void StartAttack()
    {
        hitEnemies.Clear();
        isAttacking = true;
        bladeCollider.enabled = true;
    }

    public void EndAttack()
    {
        isAttacking = false;
        bladeCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking || player == null)
            return;

        EnemyAI enemyAI = other.GetComponentInParent<EnemyAI>();

        if (enemyAI == null || !hitEnemies.Add(enemyAI))
            return;

        player.DealMeleeHit(enemyAI, other);
    }
}
