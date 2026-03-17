using UnityEngine;
using System.Collections;

public class Zombie2 : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    public float moveSpeed = 1f;
    public float attackCooldown = 1.5f;

    public int swipeDamage = 1;
    public int biteDamage = 2;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform player;
    public Animator animator;

    [Header("Drops")]
    public GameObject[] dropPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.08f; // 8% per prefab

    [Header("Hitboxes")]
    public Vector2 swipeHitOffset = new Vector2(0.8f, 0f); // local offset; X will flip with facing
    public Vector2 swipeHitSize = new Vector2(1.0f, 1.0f);
    public Vector2 biteHitOffset = new Vector2(0.5f, 0f);
    public Vector2 biteHitSize = new Vector2(0.8f, 1.0f);

    // runtime flags controlled by animation events
    private bool swipeHitboxActive = false;
    private bool biteHitboxActive = false;

    private float attackTimer = 0f;
    private float randomStopTimer = 0f;
    private bool isRandomStopping = false;
    private bool lastAttackWasBite = false;
    private bool isAttacking = false; // Tracks if zombie is mid-attack

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

        // Handle random stop
        if (randomStopTimer <= 0f)
        {
            isRandomStopping = !isRandomStopping;
            randomStopTimer = Random.Range(1f, 3f);
        }

        // Check hitboxes to decide attack
        bool playerInBite = IsPlayerInBox(biteHitOffset, biteHitSize);
        bool playerInSwipe = IsPlayerInBox(swipeHitOffset, swipeHitSize);

        // Prevent movement while attacking
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // changed linearVelocity -> velocity
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            return;
        }

        // UPDATED: alternate attacks but still allow normal movement
        if (attackTimer >= attackCooldown)
        {
            if (playerInBite && !lastAttackWasBite)
            {
                StartCoroutine(Bite());
                return; // ADDED: prevent movement same frame
            }

            if (playerInSwipe)
            {
                StartCoroutine(Swipe());
                return; // ADDED
            }
        }

        // ADDED: fallback movement when not attacking
        MoveTowardsPlayer(!isRandomStopping);

        // Walking animation only when moving and not attacking
        if (animator != null)
        {
            animator.SetBool("isWalking", rb.linearVelocity.x != 0f && !isAttacking); // changed linearVelocity -> velocity
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
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y); // changed linearVelocity -> velocity
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1f, 1f);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // changed linearVelocity -> velocity
        }
    }

    private IEnumerator Swipe()
    {
        isAttacking = true;
        attackTimer = 0f;
        lastAttackWasBite = false;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // changed linearVelocity -> velocity

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("ZombieF_Attack");
        }

        // fallback: wait clip-length or small timeout -- animation event should trigger actual hits
        yield return new WaitForSeconds(0.25f);

        // fallback hit (in case animation events not setup)
        if (!swipeHitboxActive)
        {
            // temporary enable for fallback window
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

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // changed linearVelocity -> velocity

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("ZombieF_Bite");
        }

        // fallback: wait clip-length or small timeout -- animation event should trigger actual hits
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

    // animation-event API: enable/disable hitboxes (call from clips at the desired frames)
    public void EnableSwipeHitbox() { swipeHitboxActive = true; } // animation-event API
    public void DisableSwipeHitbox() { swipeHitboxActive = false; } // animation-event API
    public void EnableBiteHitbox() { biteHitboxActive = true; } // animation-event API
    public void DisableBiteHitbox() { biteHitboxActive = false; } // animation-event API

    // Animation event handlers (use these on the attack/bite clips at the hit frame)
    public void OnSwipeHit()
    {
        if (player == null) return;
        if (!swipeHitboxActive) return; // ensure only active windows hit (fallback enables briefly above)

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
        // support animation event to clear attacking state if used
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
        // debug hitboxes
        float facing = (transform != null) ? Mathf.Sign(transform.localScale.x) : 1f;
        Vector3 swipeOrigin = transform.position + new Vector3(swipeHitOffset.x * facing, swipeHitOffset.y, 0f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(swipeOrigin, swipeHitSize);

        Vector3 biteOrigin = transform.position + new Vector3(biteHitOffset.x * facing, biteHitOffset.y, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(biteOrigin, biteHitSize);
    }
}