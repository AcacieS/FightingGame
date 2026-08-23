using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character: MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
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
        // Cached before Hp is set: the Hp setter can call Die(), which needs anim.
        anim = GetComponent<Animator>();

        if (characterInfo == null)
        {
            Debug.LogError($"{name}: CharacterInfo is not assigned.", this);
            return;
        }

        Hp = characterInfo.Hp;
    }

    public virtual void Start()
    {
        // Without this guard the NullReferenceException propagates into subclass Start
        // overrides and silently kills whatever they set up afterwards.
        if (Context.Instance == null)
        {
            Debug.LogWarning($"{name}: no Context in the scene, so LookAt is disabled.", this);
            return;
        }

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
}
