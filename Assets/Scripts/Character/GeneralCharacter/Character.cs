using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character: MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] protected Rigidbody2D rb;
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
    private Animator anim;
    public virtual void Die()
    {
        anim.SetTrigger("Death");
    }
    public virtual void Awake()
    {
        Hp = characterInfo.Hp;
        anim = GetComponent<Animator>();
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    public virtual void Start()
    {
        StartCoroutine(LookAt(Context.Instance.Target));
    }
    public void Hit(Character target, int damage)
    {
        target.Hurt(damage) ;
    }
    public void Hurt(int damage)
    {
        Hp -= damage;
        OnHurt?.Invoke(Hp);
        anim.SetTrigger("Hurt");
    }
    public IEnumerator LookAt(Character target)
    {
        if (target == null)
            yield break;

        while (true)
        {
            float direction = Context.Instance.Direction;

            if (!Mathf.Approximately(direction, 0f))
            {
                Vector3 scale = transform.localScale;

                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);

                transform.localScale = scale;
            }

            yield return null;
        }
    }
    public void Move(float direction)
    {
        float targetSpeed = direction * Info.MoveSpeed;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            Info.Acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }
}
