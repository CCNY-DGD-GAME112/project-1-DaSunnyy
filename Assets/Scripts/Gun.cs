using UnityEngine;

public class Gun : MonoBehaviour
{
    public int maxAmmo = 6;
    public int currentAmmo = 6;

    [HideInInspector]
    public PlayerController owner;

    public void Equip(PlayerController newOwner)
    {
        owner = newOwner;
        owner.gun = this;
        currentAmmo = maxAmmo;
        owner.OnGunEquipped();
    }

    public void TryFire(Vector2 direction, Vector3 spawnPos, GameObject bulletPrefab)
    {
        if (!CanFire()) return;

        if (bulletPrefab != null)
        {
            GameObject b = Object.Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            if (b.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.SetDirection(direction);
            }
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.Gun);
        }

        ConsumeAmmo();
    }

    public bool CanFire()
    {
        if (owner == null) return false;
        if (!owner.isShooting && !owner.canMove) return false;
        if (owner.isDead) return false;
        return currentAmmo > 0;
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        owner?.OnAmmoChanged(currentAmmo);
    }

    public void ConsumeAmmo(int amount = 1)
    {
        currentAmmo = Mathf.Max(0, currentAmmo - amount);
        owner?.OnAmmoChanged(currentAmmo);
    }
}