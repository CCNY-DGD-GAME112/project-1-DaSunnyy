using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 5;
    public float lifeTime = 2f;
    private Vector2 direction;
    public SpriteRenderer spriteRenderer;
    
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle -= 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spriteRenderer == null) return;

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                spriteRenderer.flipY = true;
        }
        else
        {
            if (direction.y > 0)
                spriteRenderer.flipY = true;
        }
    }

    void Start() => Destroy(gameObject, lifeTime);
    void Update() => transform.Translate(direction * speed * Time.deltaTime, Space.World);

    void OnTriggerEnter2D(Collider2D collision)
    {
        Zombie z = collision.GetComponent<Zombie>();
        Zombie2 z2 = collision.GetComponent<Zombie2>();

        if (z != null)
        {
            z.TakeDamage(damage, direction);
            Destroy(gameObject);
        }
        else if (z2 != null)
        {
            z2.TakeDamage(damage, direction);
            Destroy(gameObject);
        }
    }
}