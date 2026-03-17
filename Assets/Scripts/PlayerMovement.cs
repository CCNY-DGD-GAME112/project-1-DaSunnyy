using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float runMultiplier = 2f;
    public float jumpForce = 4f;
    public float climbSpeed = 2f;
    public Rigidbody2D rb;
    public PlayerController player;
    public bool isGrounded;
    public bool isClimbing;
    public Transform ShootPoint;

    [HideInInspector] public bool aimingUp = false;

    public Vector2 hammerHitboxOffset = new Vector2(1f, 0f);
    public Vector2 hammerHitboxSize = new Vector2(1f, 1f);

    private bool hammerPressed = false;
    private bool hammerHitboxActive = false;

    private float originalGravityScale = 1f;

    void Awake()
    {
        if (ShootPoint == null)
            ShootPoint = transform.Find("ShootPoint");
    }

    void Start()
    {
        if (player == null)
            player = GetComponent<PlayerController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;
    }

    void Update()
    {

        if (player == null) return;

        if (player.isDead) return;

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

        aimingUp = verticalInput > 0f;

        bool isRunning = Input.GetKey(KeyCode.J);
        bool jumpPressed = Input.GetKeyDown(KeyCode.K);
        bool hammerPressedNow = Input.GetKeyDown(KeyCode.L);

        Collider2D[] overlapped = Physics2D.OverlapCircleAll(transform.position, 0.8f);
        Collider2D ladder = null;
        foreach (var c in overlapped)
        {
            if (c == null) continue;
            string t = c.tag ?? "";
            if (string.Equals(t, "Ladder", System.StringComparison.OrdinalIgnoreCase))
            {
                ladder = c;
                break;
            }
        }

        if (ladder != null && Mathf.Abs(verticalInput) > 0f && !jumpPressed)
        {
            if (!isClimbing)
            {
                isClimbing = true;
                if (rb != null) rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (ladder == null)
        {
            if (isClimbing)
            {
                isClimbing = false;
                if (rb != null) rb.gravityScale = originalGravityScale;
            }
        }

        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);

            Vector3 pos = transform.position;
            pos.x = ladder != null ? ladder.transform.position.x : pos.x;
            transform.position = pos;

            if (player.playerAnimator != null && player.playerAnimator.animator != null)
                player.playerAnimator.animator.SetBool("isClimbing", Mathf.Abs(verticalInput) > 0f);

            if (jumpPressed)
            {
                isClimbing = false;
                if (rb != null) rb.gravityScale = originalGravityScale;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }

            return;
        }

        if (!isClimbing && player.playerAnimator != null && player.playerAnimator.animator != null)
            player.playerAnimator.animator.SetBool("isClimbing", false);

        if (jumpPressed && isGrounded)
        {
            if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        float speed = moveSpeed * (isRunning ? runMultiplier : 1f);
        if (rb != null) rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        if (moveInput != 0f)
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1f, 1f);

        if (hammerPressedNow && !hammerPressed)
        {
            hammerPressed = true;
            StartCoroutine(HammerAttack());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground")) isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground")) isGrounded = false;
    }

    public void EnableHammerHitbox() => hammerHitboxActive = true;
    public void DisableHammerHitbox() => hammerHitboxActive = false;

    public void ApplyHammerDamage()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        Vector2 knockback = new Vector2(facing, 1f);

        Vector2 center = (Vector2)transform.position + new Vector2(hammerHitboxOffset.x * facing, hammerHitboxOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hammerHitboxSize, 0f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Zombie zombie = hit.GetComponent<Zombie>();
            Zombie2 zombie2 = hit.GetComponent<Zombie2>();

            if (zombie != null)
            {
                zombie.TakeDamage(2, knockback);
            }
            else if (zombie2 != null)
            {
                zombie2.TakeDamage(2, knockback);
            }
        }
    }

    public void EndHammerSwing()
    {
        hammerPressed = false;
        hammerHitboxActive = false;
    }

    private IEnumerator HammerAttack()
    {
        player.playerAnimator?.PlayHammer();

        yield return new WaitForSeconds(0.1f);

        EnableHammerHitbox();

        ApplyHammerDamage();

        yield return new WaitForSeconds(0.05f);

        DisableHammerHitbox();


        hammerPressed = false;
    }

    void OnDrawGizmosSelected()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        Vector3 origin = transform.position + new Vector3(hammerHitboxOffset.x * facing, hammerHitboxOffset.y, 0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, hammerHitboxSize);
    }

    public void SetCanMove(bool value)
    {
        if (player != null)
        {
            player.canMove = value;
        }
    }

    void LateUpdate()
    {
        if (ShootPoint == null) return;

        float facing = Mathf.Sign(transform.localScale.x);

        ShootPoint.localPosition = aimingUp
            ? new Vector3(0f, 0.5f, 0f)
            : new Vector3(0.5f, 0f, 0f);

        ShootPoint.localScale = new Vector3(facing, 1f, 1f);
    }
}
