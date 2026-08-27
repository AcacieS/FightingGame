using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character: MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [Header("Debug")]
    [SerializeField] private int damageTest;
    public CharacterInfo Info => characterInfo;
    
    public event System.Action<int> OnHpChanged;
    public event System.Action<int> OnHurt;
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
    public void PlayAnim(string animName)
    {
        anim.Play(animName);
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
        Debug.Log(
            $"{name} | " +
            $"Current State: {stateInfo.fullPathHash} | " +
            $"animName: {animName} | " +
            $"normalizedTime: {stateInfo.normalizedTime} | " +
            $"inTransition: {anim.IsInTransition(0)}"
        );

        return stateInfo.IsName(animName) &&
            stateInfo.normalizedTime >= 1f &&
            !anim.IsInTransition(0);
    }
    public virtual void Die()
    {
        //anim.SetTrigger("Death");
    }
    public virtual void Awake()
    {
        // Components are cached before Hp is assigned: the Hp setter can call Die(),
        // and overrides of Die() reach for anim and rb.
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (characterInfo == null)
        {
            Debug.LogError($"{name}: CharacterInfo is not assigned, so Hp stays at 0.", this);
            return;
        }

        Hp = characterInfo.Hp;
    }

    public virtual void Start()
    {
        
    }
    public void Hit(Character target, int damage)
    {
        target.Hurt(damage) ;
    }

    [ContextMenu("Hurt Test")]
    public void HurtTest()
    {
        Hurt(damageTest);
    }
    public void Hurt(int damage)
    {
        Hp -= damage;
        OnHurt?.Invoke(Hp);
        anim.SetTrigger("Hurt");
    }
    public void LookAt(Character target)
    {
        if (target == null)
            return;

        float direction = target.transform.position.x - transform.position.x;

        if (Mathf.Approximately(direction, 0f))
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
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
}
