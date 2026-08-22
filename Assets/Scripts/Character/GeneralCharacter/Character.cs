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
        Hp = characterInfo.Hp;
        anim = GetComponent<Animator>();
    }

    public virtual void Start()
    {
        
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
}
