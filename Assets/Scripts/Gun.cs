using UnityEngine;

public class Gun : MonoBehaviour
{
    public int maxAmmo = 6;
    public int currentAmmo = 6;

    public float fireRate = 0.25f;
    private float cooldown = 0f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    private Vector2 lastFireDir = Vector2.right;

    [Header("Owner")]
    public PlayerController owner;

    void Awake()
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    public void TryFire(Vector2 dir, Vector3 spawnPos)
    {
        if (owner != null && (!owner.canMove || owner.isDead)) return;
        if (cooldown > 0f) return;
        if (currentAmmo <= 0) return;

        currentAmmo = Mathf.Max(0, currentAmmo - 1);
        cooldown = fireRate;

        lastFireDir = dir.normalized != Vector2.zero ? dir.normalized : Vector2.right;

        SpawnBullet(spawnPos);

        owner?.OnAmmoChanged(currentAmmo);
    }

    public void SpawnBullet(Vector3 spawnPos)
    {
        if (bulletPrefab == null) return;

        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        if (go.TryGetComponent<Bullet>(out var b))
        {
            b.SetDirection(lastFireDir);
        }
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        owner?.OnAmmoChanged(currentAmmo);
    }

    public void Equip(PlayerController newOwner)
    {
        owner = newOwner;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        owner?.OnGunEquipped();
    }

    public bool CanFire() => cooldown <= 0f && currentAmmo > 0 && (owner == null || (owner.canMove && !owner.isDead));
}