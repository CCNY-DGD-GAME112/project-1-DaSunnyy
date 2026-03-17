using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        Food,
        Medkit,
        Ammo,
        Gun,
        Antidote
    }

    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<PlayerController>(out var player))
            return;

        switch (itemType)
        {
            case ItemType.Food:
                player.Heal(2);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.Heal);
                break;

            case ItemType.Medkit:
                player.Heal(player.maxHealth);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.Heal);
                break;

            case ItemType.Ammo:
                if (player.gun != null)
                    player.gun.AddAmmo(player.gun.maxAmmo);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.Ammo);
                break;

            case ItemType.Gun:
                player.EquipGun();
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.Ammo);
                break;

            case ItemType.Antidote:
                player.StopZombification();
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.Heal);
                break;
        }
        Destroy(gameObject);
    }
}
