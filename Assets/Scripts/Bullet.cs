using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public int damage = 5;
    public float lifeTime = 2f;

    private Vector2 direction;

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spriteRenderer != null)
        {
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
            else
            {

                spriteRenderer.flipX = false;
            }
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Zombie zombie = collision.GetComponent<Zombie>();
        Zombie2 zombie2 = collision.GetComponent<Zombie2>();

        Vector2 knockback = direction;

        if (zombie != null)
        {
            zombie.TakeDamage(damage, knockback);
            Destroy(gameObject);
        }
        else if (zombie2 != null)
        {
            zombie2.TakeDamage(damage, knockback);
            Destroy(gameObject);
        }
    }
}