using UnityEngine;

public class Context : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Character self;
    [SerializeField] private Character target;
    public static Context Instance { get; private set; }

    public Character Self => self;
    public Character Target => target;

    // Runtime data
    public float Distance { get; private set; }

    public float SelfHp => Self != null ? Self.Hp : 0;
    public float SelfMaxHp => Self != null ? Self.Info.Hp : 0;

    public float TargetHp => Target != null ? Target.Hp : 0;
    public float TargetMaxHp => Target != null ? Target.Info.Hp : 0;

    public bool IsLowHealth => SelfHp < 30;
    public bool TargetIsLowHealth => TargetHp < 30;
    public bool IsInAttackRange => Distance <= 2f;

    public bool TargetIsAttacking { get; set; }
    public bool TargetIsBlocking { get; set; }

    public void Update()
    {
        if (Self == null || Target == null)
            return;

        Distance = Vector3.Distance(
            Self.transform.position,
            Target.transform.position
        );
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}