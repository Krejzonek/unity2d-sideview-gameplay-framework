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

    private Vector2 direction;
    private float fallSpeed;
    private bool stuck;

    public void Setup(Vector2 direction, float damage, float critRate, float critDamage, float knockback, float stunChance, ElementType element)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.critRate = critRate;
        this.critDamage = critDamage;
        this.knockback = knockback;
        this.stunChance = stunChance;
        this.element = element;

        transform.right = this.direction;

        Destroy(gameObject, lifetime);
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
            return;
        }

        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();

        if (enemy != null)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            bool isCrit = Random.Range(0f, 100f) <= critRate;
            float finalDamage = damage;

            if (isCrit)
                finalDamage *= critDamage;

            float elementMultiplier = ElementSystem.GetDamageMultiplier(element, enemy.GetElement());
            finalDamage *= elementMultiplier;

            Vector2 knockbackDirection = direction.normalized;
            enemy.TakeDamage(finalDamage, hitPoint, knockback, knockbackDirection, stunChance, isCrit);
            Destroy(gameObject);
        }
    }
}