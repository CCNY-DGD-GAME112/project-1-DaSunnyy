using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerMovement movement;
    public PlayerController player;
    private bool isLocked = false;
    private bool hammerSwinging = false;

    private static readonly int PlayerIdle = Animator.StringToHash("PlayerIdle");
    private static readonly int PlayerWalk = Animator.StringToHash("PlayerWalk");
    private static readonly int PlayerRun = Animator.StringToHash("PlayerRun");
    private static readonly int PlayerJumpStart = Animator.StringToHash("PlayerJumpStart");
    private static readonly int PlayerJumpPeak = Animator.StringToHash("PlayerJumpPeak");
    private static readonly int PlayerFall = Animator.StringToHash("PlayerFall");
    private static readonly int PlayerClimb = Animator.StringToHash("PlayerClimb");
    private static readonly int PlayerHammerStart = Animator.StringToHash("PlayerHammerStart");
    private static readonly int PlayerHammer = Animator.StringToHash("PlayerHammer");
    private static readonly int PlayerHurt = Animator.StringToHash("PlayerHurt");
    private static readonly int PlayerDefeat = Animator.StringToHash("PlayerDefeat");
    private static readonly int PlayerDie = Animator.StringToHash("PlayerDie");
    private static readonly int PlayerZombify = Animator.StringToHash("PlayerZombify");
    private static readonly int PlayerZombie = Animator.StringToHash("PlayerZombie");

    private static readonly int PlayerGun = Animator.StringToHash("PlayerGun");
    private static readonly int PlayerGunWalk = Animator.StringToHash("PlayerGunWalk");
    private static readonly int PlayerGunRun = Animator.StringToHash("PlayerGunRun");
    private static readonly int PlayerGunUp = Animator.StringToHash("PlayerGunUp");
    private static readonly int PlayerGunWalkUp = Animator.StringToHash("PlayerGunWalkUp");
    private static readonly int PlayerGunClimb = Animator.StringToHash("PlayerGunClimb");
    private static readonly int PlayerGunClimbUp = Animator.StringToHash("PlayerGunClimbUp");
    private static readonly int PlayerGunFall = Animator.StringToHash("PlayerGunFall");

    private int currentState = -1;

    void Update()
    {
        if (animator == null || movement == null || player == null) return;

        if (isLocked || player.isDead) return;

        if (hammerSwinging) return;

        int newState;

        if (player.isShooting)
            newState = GetGunState();
        else
            newState = GetState();

        if (newState != currentState)
        {
            animator.CrossFade(newState, 0.05f);
            currentState = newState;
        }
    }

    private int GetState()
    {
        float xVel = Mathf.Abs(movement.rb.linearVelocity.x);
        float yVel = movement.rb.linearVelocity.y;
        bool grounded = movement.isGrounded;
        bool climbing = movement.isClimbing;

        if (climbing) return PlayerClimb;
        if (!grounded)
        {
            if (yVel > 0.1f) return PlayerJumpStart;
            if (yVel < -1.5f) return PlayerFall;
            return PlayerJumpPeak;
        }
        if (xVel > 2.1f) return PlayerRun;
        if (xVel > 0.1f) return PlayerWalk;
        return PlayerIdle;
    }

    private int GetGunState()
    {
        float xVel = Mathf.Abs(movement.rb.linearVelocity.x);
        float yVel = movement.rb.linearVelocity.y;
        bool grounded = movement.isGrounded;
        bool climbing = movement.isClimbing;
        bool aimingUp = movement.aimingUp;

        if (climbing)
        {
            if (aimingUp) return PlayerGunClimbUp;
            return PlayerGunClimb;
        }

        if (!grounded)
        {
            if (aimingUp) return PlayerGunUp;
            return PlayerGunFall;
        }

        if (aimingUp)
        {
            if (xVel > 0.1f) return PlayerGunWalkUp;
            return PlayerGunUp;
        }

        if (xVel > 2.1f) return PlayerGunRun;
        if (xVel > 0.1f) return PlayerGunWalk;

        return PlayerGun;
    }

    public void PlayHurt()
    {
        if (animator == null) return;

        animator.SetBool("isHurt", true);
        Invoke(nameof(ResetHurt), 1f);
    }

    private void ResetHurt()
    {
        animator.SetBool("isHurt", false);
    }

    public void PlayDie()
    {
        if (animator != null)
        {
            isLocked = true;
            animator.Play(PlayerDefeat);
            currentState = PlayerDefeat;
            StartCoroutine(PlayDieSequence());
        }
    }

    private IEnumerator PlayDieSequence()
    {
        if (animator == null) yield break;

        yield return null;
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        float defeatLen = (clips != null && clips.Length > 0 && clips[0].clip != null)
            ? clips[0].clip.length / Mathf.Max(animator.speed, 1f)
            : 0.5f;
        yield return new WaitForSeconds(defeatLen);

        animator.Play(PlayerDie);
        currentState = PlayerDie;
        yield return null;
        clips = animator.GetCurrentAnimatorClipInfo(0);
        float dieLen = (clips != null && clips.Length > 0 && clips[0].clip != null)
            ? clips[0].clip.length / Mathf.Max(animator.speed, 1f)
            : 1f;
        yield return new WaitForSeconds(dieLen);

        isLocked = false;
        currentState = -1;
        player?.OnDeathSequenceComplete();
    }

    public void PlayZombify()
    {
        if (animator != null)
        {
            isLocked = true;
            animator.Play(PlayerZombify);
            currentState = PlayerZombify;
            StartCoroutine(PlayZombifySequence());
        }
    }

    private IEnumerator PlayZombifySequence()
    {
        if (animator == null) yield break;

        yield return null;
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        float z1 = (clips != null && clips.Length > 0 && clips[0].clip != null)
            ? clips[0].clip.length / Mathf.Max(animator.speed, 1f)
            : 0.5f;
        yield return new WaitForSeconds(z1);

        animator.Play(PlayerZombie);
        currentState = PlayerZombie;

        yield return null;
        clips = animator.GetCurrentAnimatorClipInfo(0);
        float z2 = (clips != null && clips.Length > 0 && clips[0].clip != null)
            ? clips[0].clip.length / Mathf.Max(animator.speed, 1f)
            : 1f;
        yield return new WaitForSeconds(z2);

        isLocked = false;
        currentState = -1;
        player?.OnZombifySequenceComplete();
    }

    public void PlayHammer()
    {
        if (animator == null || hammerSwinging) return;
        StartCoroutine(PlayHammerSwing());
    }

    private IEnumerator PlayHammerSwing()
    {
        if (animator == null || hammerSwinging) yield break;

        hammerSwinging = true;
        isLocked = true;

        animator.Play(PlayerHammerStart, 0, 0f);

        yield return null;

        float startLen = 0.08f;
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0 && clips[0].clip != null)
            startLen = clips[0].clip.length / Mathf.Max(animator.speed, 1f);

        yield return new WaitForSeconds(startLen);

        animator.Play(PlayerHammer, 0, 0f);

        yield return null;

        clips = animator.GetCurrentAnimatorClipInfo(0);
        float mainLen = 0.18f;
        if (clips.Length > 0 && clips[0].clip != null)
            mainLen = clips[0].clip.length / Mathf.Max(animator.speed, 1f);

        yield return new WaitForSeconds(mainLen);

        hammerSwinging = false;
        isLocked = false;
        currentState = -1;
        movement?.EndHammerSwing();
    }

    public void OnHammerHit()
    {
        movement?.ApplyHammerDamage();
    }

    public void OnHammerEnd()
    {
        movement?.DisableHammerHitbox();
        hammerSwinging = false;
        currentState = -1;
        movement?.EndHammerSwing();
    }

    public void PlayGunFire()
    {
        if (animator != null)
        {
            animator.CrossFade(PlayerGun, 0f);
            currentState = PlayerGun;
        }
    }

    public void UpdateAnimatorStates()
    {
        if (animator == null || movement == null || player == null) return;

        int newState = player.gun != null ? GetGunState() : GetState();

        if (newState != currentState)
        {
            animator.CrossFade(newState, 0.05f);
            currentState = newState;
        }
    }
}