using UnityEngine;
using System.Collections;

public class Zombie : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float moveSpeed = 1f;
    public float attackCooldown = 1.5f;

    public int swipeDamage = 1;
    public int biteDamage = 2;

    public Rigidbody2D rb;
    public Transform player;
    public Animator animator;

    public GameObject[] dropPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.08f;

    public Vector2 swipeHitOffset = new Vector2(0.8f, 0f);
    public Vector2 swipeHitSize = new Vector2(1.0f, 1.0f);
    public Vector2 biteHitOffset = new Vector2(0.5f, 0f);
    public Vector2 biteHitSize = new Vector2(0.8f, 1.0f);

    private bool swipeHitboxActive = false;
    private bool biteHitboxActive = false;

    private float attackTimer = 0f;
    private float randomStopTimer = 0f;
    private bool isRandomStopping = false;
    private bool lastAttackWasBite = false;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                player = pObj.transform;
            }
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        randomStopTimer = Random.Range(1f, 3f);
    }

    void Update()
    {
        if (player == null) return;

        attackTimer += Time.deltaTime;
        randomStopTimer -= Time.deltaTime;

        if (randomStopTimer <= 0f)
        {
            isRandomStopping = !isRandomStopping;
            randomStopTimer = Random.Range(1f, 3f);
        }

        bool playerInBite = IsPlayerInBox(biteHitOffset, biteHitSize);
        bool playerInSwipe = IsPlayerInBox(swipeHitOffset, swipeHitSize);

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            return;
        }

        if (attackTimer >= attackCooldown)
        {
            if (playerInBite && !lastAttackWasBite)
            {
                StartCoroutine(Bite());
                return;
            }

            if (playerInSwipe)
            {
                StartCoroutine(Swipe());
                return;
            }
        }

        MoveTowardsPlayer(!isRandomStopping);

        if (animator != null)
        {
            animator.SetBool("isWalking", rb.linearVelocity.x != 0f && !isAttacking);
        }
    }

    bool IsPlayerInBox(Vector2 offset, Vector2 size)
    {
        float facing = Mathf.Sign(transform.localScale.x);
        Vector3 origin = transform.position + new Vector3(offset.x * facing, offset.y, 0f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f);
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (string.Equals(c.tag, "Player", System.StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    void MoveTowardsPlayer(bool canMove)
    {
        if (canMove)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1f, 1f);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private IEnumerator Swipe()
    {
        isAttacking = true;
        attackTimer = 0f;
        lastAttackWasBite = false;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("ZombieM_Attack");
        }

        yield return new WaitForSeconds(0.25f);

        if (!swipeHitboxActive)
        {
            swipeHitboxActive = true;
            OnSwipeHit();
            swipeHitboxActive = false;
        }
        else
        {
            OnSwipeHit();
        }

        isAttacking = false;
    }

    private IEnumerator Bite()
    {
        isAttacking = true;
        attackTimer = 0f;
        lastAttackWasBite = true;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("ZombieM_Bite");
        }

        yield return new WaitForSeconds(0.35f);

        if (!biteHitboxActive)
        {
            biteHitboxActive = true;
            OnBiteHit();
            biteHitboxActive = false;
        }
        else
        {
            OnBiteHit();
        }

        isAttacking = false;
    }

    public void EnableSwipeHitbox() { swipeHitboxActive = true; }
    public void DisableSwipeHitbox() { swipeHitboxActive = false; }
    public void EnableBiteHitbox() { biteHitboxActive = true; }
    public void DisableBiteHitbox() { biteHitboxActive = false; }

    public void OnSwipeHit()
    {
        if (player == null) return;
        if (!swipeHitboxActive) return;

        float facing = Mathf.Sign(transform.localScale.x);
        Vector3 origin = transform.position + new Vector3(swipeHitOffset.x * facing, swipeHitOffset.y, 0f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, swipeHitSize, 0f);
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (!string.Equals(c.tag, "Player", System.StringComparison.OrdinalIgnoreCase)) continue;

            if (c.TryGetComponent(out PlayerController p))
            {
                p.TakeDamage(Mathf.Min(swipeDamage, p.currentHealth), Vector2.zero);
            }
        }
    }

    public void OnBiteHit()
    {
        if (player == null) return;

        if (!biteHitboxActive) return;

        float facing = Mathf.Sign(transform.localScale.x);
        Vector3 origin = transform.position + new Vector3(biteHitOffset.x * facing, biteHitOffset.y, 0f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, biteHitSize, 0f);
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (!string.Equals(c.tag, "Player", System.StringComparison.OrdinalIgnoreCase)) continue;

            if (c.TryGetComponent(out PlayerController p))
            {
                p.TakeDamage(Mathf.Min(biteDamage, p.currentHealth), Vector2.zero);
                p.StartZombification();
            }
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    public void TakeDamage(int dmg, Vector2 knockback)
    {
        currentHealth -= dmg;
        rb.AddForce(knockback * 2f, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;

            SpawnDrops();

            Destroy(gameObject);
        }
    }

    private void SpawnDrops()
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0) return;

        foreach (var prefab in dropPrefabs)
        {
            if (prefab == null) continue;
            if (Random.value <= dropChance)
            {
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.1f, 0f);
                Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        float facing = (transform != null) ? Mathf.Sign(transform.localScale.x) : 1f;
        Vector3 swipeOrigin = transform.position + new Vector3(swipeHitOffset.x * facing, swipeHitOffset.y, 0f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(swipeOrigin, swipeHitSize);

        Vector3 biteOrigin = transform.position + new Vector3(biteHitOffset.x * facing, biteHitOffset.y, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(biteOrigin, biteHitSize);
    }
}