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
                break;

            case ItemType.Medkit:
                player.Heal(player.maxHealth);
                break;

            case ItemType.Ammo:
                if (player.gun != null)
                    player.gun.AddAmmo(player.gun.maxAmmo);
                break;

            case ItemType.Gun:
                player.EquipGun();
                break;

            case ItemType.Antidote:
                player.StopZombification();
                break;
        }

        Destroy(gameObject);
    }
}