using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public Image heartImage;
    public Sprite[] heartSprites;

    public void UpdateHearts(int health)
    {
        if (heartSprites == null || heartSprites.Length == 0 || heartImage == null) return;

        int idx = Mathf.Clamp(health, 0, heartSprites.Length - 1);
        heartImage.sprite = heartSprites[idx];
    }

    public void PlayHeartHurt(int health)
    {
        UpdateHearts(health);
    }
}