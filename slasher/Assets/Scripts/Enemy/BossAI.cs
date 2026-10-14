using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDuration = 1f;
    [SerializeField] private float chargeCooldown = 1.5f;
    [SerializeField] private float knockbackForce = 20f;

    private EnemyAI enemyAI;
    private Rigidbody2D rb;

    private Transform player;
    private Player playerScript;

    private float chargeTimer;
    private float cooldownTimer;
    private bool isCharging;
    private bool hasHitPlayer;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player == null)
        {
            player = enemyAI.GetPlayer();
            playerScript = enemyAI.GetPlayerScript();

            if (player == null)
                return;
        }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (isCharging)
        {
            chargeTimer -= Time.deltaTime;

            if (chargeTimer <= 0f)
                StopCharge();

            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= enemyAI.GetDetectionRange() && cooldownTimer <= 0f)
            StartCharge();
    }

    private void StartCharge()
    {
        isCharging = true;
        hasHitPlayer = false;
        chargeTimer = chargeDuration;
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * chargeSpeed, rb.velocity.y);
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void StopCharge()
    {
        isCharging = false;
        rb.velocity = new Vector2(0f, rb.velocity.y);
        cooldownTimer = chargeCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCharging)
            return;

        Player hitPlayer = collision.gameObject.GetComponent<Player>();
        if (hitPlayer == null) hitPlayer = collision.gameObject.GetComponentInParent<Player>();

        if (hitPlayer != null && !hasHitPlayer)
        {
            hasHitPlayer = true;
            Vector2 direction = (hitPlayer.transform.position - transform.position).normalized;
            hitPlayer.TakeDamage(enemyAI.GetDamage());
            hitPlayer.ApplyKnockback(direction, knockbackForce);

            StopCharge();
        }
        else
        {
            StopCharge();
        }
    }
}