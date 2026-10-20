using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float gravity = 3f;
    [SerializeField] private LayerMask groundLayer;

    private float damage;
    private float critRate;
    private float critDamage;
    private float knockback;
    private float stunChance;
    private ElementType element;
    private StatusEffectType statusEffect;
    private float statusChance;
    private float statusDuration;
    private float statusDamage;

    private bool isEnemyProjectile;
    private Player owner;

    private Vector2 direction;
    private Vector2 startPosition;
    private float fallSpeed;
    private bool stuck;

    public void Setup(Vector2 direction, float damage, float critRate, float critDamage, float knockback, float stunChance, ElementType element, StatusEffectType statusEffect, float statusChance, float statusDuration, float statusDamage, bool isEnemyProjectile = false, Player owner = null)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.critRate = critRate;
        this.critDamage = critDamage;
        this.knockback = knockback;
        this.stunChance = stunChance;
        this.element = element;
        this.statusEffect = statusEffect;
        this.statusChance = statusChance;
        this.statusDuration = statusDuration;
        this.statusDamage = statusDamage;
        this.isEnemyProjectile = isEnemyProjectile;
        this.owner = owner;

        startPosition = transform.position;

        transform.right = this.direction;

        Destroy(gameObject, lifetime);
    }

    public void AlignTipToPoint(Transform point)
    {
        if (point == null)
            return;

        transform.right = direction;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Bounds bounds = spriteRenderer.sprite.bounds;
        Vector3 tipOffset = transform.right * bounds.max.x * Mathf.Abs(spriteRenderer.transform.lossyScale.x);

        transform.position = point.position - tipOffset;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (stuck)
            return;

        fallSpeed += gravity * Time.deltaTime;
        Vector2 movement = direction * speed;
        movement.y -= fallSpeed;
        transform.position += (Vector3)(movement * Time.deltaTime);

        if (movement.sqrMagnitude > 0.01f)
            transform.right = movement.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            stuck = true;

            Collider2D collider = GetComponent<Collider2D>();

            if (collider != null)
                collider.enabled = false;

            return;
        }

        if (stuck)
            return;

        if (isEnemyProjectile)
        {
            Player player = other.GetComponentInParent<Player>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            EnemyAI enemy = other.GetComponentInParent<EnemyAI>();

            if (enemy != null)
                return;
        }
        else
        {
            EnemyAI enemy = other.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                float distance = Vector2.Distance(startPosition, enemy.transform.position);

                Vector2 hitPoint = other.ClosestPoint(transform.position);
                bool isCrit = Random.Range(0f, 100f) <= critRate;
                float finalDamage = damage;

                if (isCrit)
                    finalDamage *= critDamage;

                float elementMultiplier = ElementSystem.GetDamageMultiplier(element, enemy.GetElement());
                finalDamage *= elementMultiplier;

                if (owner != null)
                {
                    ClassSkillManager skillManager = owner.GetComponent<ClassSkillManager>();

                    if (skillManager != null)
                    {
                        float skillMultiplier = skillManager.GetDamageMultiplier(distance);
                        finalDamage *= skillMultiplier;
                    }
                }

                Vector2 knockbackDirection = direction.normalized;

                enemy.TakeDamage(finalDamage, hitPoint, knockback, knockbackDirection, stunChance, isCrit, statusEffect, statusChance, statusDuration, statusDamage, owner, element);

                if (owner != null)
                {
                    ClassSkillManager skillManager = owner.GetComponent<ClassSkillManager>();

                    if (skillManager != null)
                        skillManager.RegisterHit();
                }

                Destroy(gameObject);
            }
        }
    }
}