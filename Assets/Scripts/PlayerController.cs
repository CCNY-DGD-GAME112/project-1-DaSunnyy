using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 8;
    public int currentHealth;

    public bool canMove = true;

    public PlayerAnimator playerAnimator;
    public HeartUI heartUI;
    public Gun gun;
    public GameObject bulletPrefab;
    public PlayerMovement movement;
    public bool isShooting = false;

    public TMP_Text zombifyTimerText;

    public Rigidbody2D rb;

    public bool hasAntidote = false;
    public float zombifyDuration = 25f;
    public float zombifyTimer;

    public float fireCooldown = 0.2f;
    private float fireTimer = 0f;

    public bool isDead { get; private set; } = false;
    public bool isZombifying { get; private set; } = false;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        playerAnimator ??= GetComponent<PlayerAnimator>();
        movement ??= GetComponent<PlayerMovement>();
        heartUI?.UpdateHearts(currentHealth);

        if (zombifyTimerText != null)
            zombifyTimerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        HandleZombification();

        if (gun == null || movement == null) return;

        fireTimer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            isShooting = true;
            playerAnimator?.animator.SetTrigger(movement.aimingUp ? "isShootingUp" : "isShooting");
        }
        else
        {
            isShooting = false;
        }
    }

    public void TakeDamage(int amount) => TakeDamage(amount, Vector2.zero);

    public void TakeDamage(int dmg, Vector2 knockback)
    {
        if (isDead) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (rb != null && knockback != Vector2.zero)
            rb.AddForce(knockback, ForceMode2D.Impulse);

        heartUI?.UpdateHearts(currentHealth);

        if (playerAnimator != null && currentHealth > 0)
            playerAnimator.PlayHurt();

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            canMove = false;
            playerAnimator?.PlayDie();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        heartUI?.UpdateHearts(currentHealth);
    }

    public void StartZombification()
    {
        if (isDead || isZombifying || hasAntidote) return;

        isZombifying = true;
        zombifyTimer = zombifyDuration;

        if (zombifyTimerText != null)
        {
            zombifyTimerText.gameObject.SetActive(true);
            zombifyTimerText.text = zombifyTimer.ToString("F0");
        }

        if (playerAnimator?.animator != null)
            playerAnimator.animator.SetFloat("ZombifyTimer", zombifyTimer);
    }

    public void StopZombification()
    {
        isZombifying = false;
        zombifyTimer = 0f;

        if (zombifyTimerText != null)
            zombifyTimerText.gameObject.SetActive(false);

        if (playerAnimator?.animator != null)
            playerAnimator.animator.SetFloat("ZombifyTimer", 0f);
    }

    private void HandleZombification()
    {
        if (!isZombifying || hasAntidote) return;

        zombifyTimer -= Time.deltaTime;

        if (zombifyTimerText != null)
            zombifyTimerText.text = zombifyTimer.ToString("F0");

        if (playerAnimator?.animator != null)
            playerAnimator.animator.SetFloat("ZombifyTimer", zombifyTimer);

        if (zombifyTimer <= 0f && !isDead)
        {
            isDead = true;
            canMove = false;
            playerAnimator?.PlayZombify();
        }
    }

    public void OnDeathSequenceComplete()
    {
        StartCoroutine(RestartSceneAfterDelay(3f));
    }

    public void OnZombifySequenceComplete()
    {
        StartCoroutine(RestartSceneAfterDelay(3f));
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddAmmo(int amount)
    {
        if (gun != null)
        {
            gun.AddAmmo(amount);
            return;
        }

        var g = GetComponentInChildren<Gun>();
        if (g != null)
        {
            g.AddAmmo(amount);
        }
    }

    public void EquipGun()
    {
        if (gun != null)
        {
            gun.Equip(this);
            canMove = true;
            return;
        }

        var g = GetComponentInChildren<Gun>();
        if (g != null)
        {
            gun = g;
            gun.Equip(this);
            canMove = true;
            return;
        }
    }

    public void OnAmmoChanged(int newAmmo)
    {

    }

    public void OnGunEquipped()
    {

    }

    IEnumerator ResetShoot()
    {
        yield return new WaitForSeconds(0.2f);
        isShooting = false;
    }

    public void ShootFromAnimationEvent()
    {
        if (gun == null || movement == null || bulletPrefab == null) return;

        Vector2 dir = movement.aimingUp ? Vector2.up : new Vector2(Mathf.Sign(transform.localScale.x), 0f);
        gun.TryFire(dir, movement.ShootPoint.position, bulletPrefab);
    }

    public void SpawnBulletEvent()
    {
        if (gun == null || movement == null || movement.ShootPoint == null || bulletPrefab == null)
        {
            Debug.Log("Cannot spawn bullet! Missing references.");
            return;
        }

        if (!gun.CanFire())
        {
            Debug.Log($"Cannot fire! Ammo: {gun.currentAmmo}, owner: {gun.owner}, canMove: {gun.owner?.canMove}, isDead: {gun.owner?.isDead}");
            return;
        }

        Vector2 dir = movement.aimingUp ? Vector2.up : new Vector2(Mathf.Sign(transform.localScale.x), 0f);

        gun.TryFire(dir, movement.ShootPoint.position, bulletPrefab);
    }
}
