using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character: MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator anim;
    [Header("Debug")]
    [SerializeField] private int damageTest;
    [SerializeField] private bool isInterruptibleTest;
    [SerializeField] protected float groundCheckDistance = 0.2f;
    [Header("Jump")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected Collider2D characterCollider;
    private bool isDead = false;
    public bool IsDead => isDead;
    public Rigidbody2D Rb => rb;

    public bool IsOnGround
    {
        get
        {
            if (characterCollider == null)
                return false;

            Bounds bounds = characterCollider.bounds;

            RaycastHit2D hit = Physics2D.BoxCast(
                bounds.center,
                bounds.size,
                0f,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            return hit.collider != null;
        }
    }
    public CharacterInfo Info => characterInfo;
    
    public event System.Action<int> OnHpChanged;
    public event System.Action<int, bool> OnHurt;
    //public bool OnGround=>
    private int hp;
    public int Hp
    {
        get => hp;
        private set
        {
            hp = value;
            // TODO: UI Health Bar Logic
            OnHpChanged?.Invoke(hp);
            if (hp <= 0)
            {
                Die();
            }
        }
    }
    public virtual void PlayReadyAnim()
    {
        //Do Ready Animation
    }
    
    public virtual void StartCharacterMatch()
    {
        //TODO Allow Player to move and all
    }
    
    public virtual void Die()
    {
        isDead = true;
        Debug.Log("Die");
        anim.SetTrigger("Death");
    }
    public virtual void Awake()
    {
        Hp = characterInfo.Hp;
        if (anim == null){
            anim = GetComponent<Animator>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if(characterCollider == null)
        {
            characterCollider = GetComponent<Collider2D>();
        }
    }
    

    public virtual void Start()
    {
        
    }
    // isInterruptible rides along so a heavy move can stagger the victim while a light
    // one only chips it. Defaulted, so existing two-argument calls are unaffected.
    public void Hit(Character target, int damage, bool isInterruptible = false)
    {
        target.Hurt(damage, isInterruptible);
    }

    [ContextMenu("Hurt Test")]
    public void HurtTest()
    {
        Hurt(damageTest, isInterruptibleTest);
    }
    public virtual bool Hurt(int damage, bool isInterruptible = false, float stunDuration = 0f)
    {
        Hp -= damage;
        OnHurt?.Invoke(Hp, isInterruptible);
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
        if(Hp <= 0)
        {
            Die();
        }
        return true;
    }
    public void LookAt(Character target, bool isInverse = false)
    {
        if (target == null)
            return;

        float direction =
            target.transform.position.x - transform.position.x;

        if (Mathf.Approximately(direction, 0f))
            return;

        if (isInverse)
            direction = -direction;

        Vector3 scale = transform.localScale;

        scale.x =
            Mathf.Abs(scale.x) * Mathf.Sign(direction);

        transform.localScale = scale;
    }

    public IEnumerator LookAtContinuously(Character target)
    {
        if (target == null)
            yield break;

        while (true)
        {
            LookAt(target);
            yield return null;
        }
    }
    public void Move(float direction)
    {
        Move(direction, Info.MoveSpeed, Info.Acceleration);
    }
    public void Move(float direction, float speed, float acceleration)
    {
        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D is NULL!");
            return;
        }

        if (Info == null)
        {
            Debug.LogError($"{name}: CharacterInfo is NULL!");
            return;
        }

        // Debug.Log(
        //     $"{name} Move | " +
        //     $"direction={direction} | " +
        //     $"speed={Info.MoveSpeed} | " +
        //     $"acceleration={Info.Acceleration} | " +
        //     $"before={rb.linearVelocity}"
        // );

        float targetSpeed = direction * speed;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );

        //Debug.Log($"AFTER velocity = {rb.linearVelocity}");
    }
    public void StopMoving()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }
    //---------------------------------- ANIMATOR ---------------------------
    public void PlayAnim(string animName, float desiredDuration = 0)
    {
        if(anim == null) return;
        if (desiredDuration != 0)
        {
            SetAnimationSpeed(animName, desiredDuration);
        }
        anim.Play(animName, 0, 0f);
    }
    private void SetAnimationSpeed(string animName, float desiredDuration)
    {
        RuntimeAnimatorController controller =
            anim.runtimeAnimatorController;

        if (controller == null)
            return;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name != animName)
                continue;

            float originalDuration = clip.length;

            if (originalDuration <= 0f)
                return;

            anim.speed =
                originalDuration / desiredDuration;

            Debug.Log(
                $"{name}: {animName} | " +
                $"Original: {originalDuration:F2}s | " +
                $"Desired: {desiredDuration:F2}s | " +
                $"Speed: {anim.speed:F2}x"
            );

            return;
        }

        Debug.LogWarning(
            $"{name}: Could not find animation clip '{animName}'."
        );
    }
    public bool IsAnimPlaying(string animName)
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animName);
    }
    public bool IsAnimFinished(string animName)
    {
        if (anim == null)
            return false;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        // Debug.Log(
        //     $"{name} | " +
        //     $"Current State: {stateInfo.fullPathHash} | " +
        //     $"animName: {animName} | " +
        //     $"normalizedTime: {stateInfo.normalizedTime} | " +
        //     $"inTransition: {anim.IsInTransition(0)}"
        // );

        return stateInfo.IsName(animName) &&
            stateInfo.normalizedTime >= 1f &&
            !anim.IsInTransition(0);
    }
}
