using System;
using UnityEngine;

public class Context : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private bool _overrideCharactersSettings = false;
    [SerializeField] private Character _self;
    [SerializeField] private Character _target;
    [SerializeField] private AIController _selfController;
    [ReadOnly, SerializeField] private State selfState;
    public static Context Instance { get; private set; }
    public Character Self => _self;
    public Character Target => _target;
    public AIController AIController => AIController;

    public State SelfState => selfState;
    public void SetCurrentState(State newSelfState) => selfState = newSelfState;

    // Runtime data
    public float Distance { get; private set; }
    public float DistanceX { get; private set; }
    public float Direction { get; private set; }
    public float DirectionSign => Mathf.Sign(Direction);
    public float AngleToTarget { get; private set; }

    public bool TargetIsAbove45 => AngleToTarget > 45f && AngleToTarget < 135f;
    public bool TargetIsBelow45 => AngleToTarget < -45f && AngleToTarget > -135f;
    public bool TargetIsInFront45
    {
        get
        {
            return Mathf.Abs(AngleToTarget) <= 45f;
        }
    }
    public float SelfHp => Self != null ? Self.Hp : 0;
    public float SelfMaxHp => Self != null ? Self.Info.Hp : 0;

    public float TargetHp => Target != null ? Target.Hp : 0;
    public float TargetMaxHp => Target != null ? Target.Info.Hp : 0;

    public bool IsLowHealth => SelfHp < 30;
    public bool TargetIsLowHealth => TargetHp < 30;
    public bool IsInAttackRange => Distance <= 2f;

    public bool TargetIsAttacking { get; set; }
    public bool TargetIsBlocking { get; set; }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMatchStart += HandleMatchChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMatchStart -= HandleMatchChanged;
    }
    public void HandleMatchChanged()
    {
        Match currentMatch = GameManager.Instance.Match;
        if (!_overrideCharactersSettings)
        {
            _self = currentMatch.Enemy;
            _target = currentMatch.Player;
            _selfController = currentMatch.Enemy.AIController;
            if (_self == null||_target==null||_selfController==null)
            {
                Debug.LogError("Self or Target or SelfController: null");
            }
        }
    }
    

    public void Update()
    {
        if (Self == null || Target == null)
        return;

        Vector2 toTarget =
            Target.transform.position -
            Self.transform.position;

        Distance = toTarget.magnitude;

        DistanceX = Mathf.Abs(toTarget.x);

        Direction = toTarget.x;

        AngleToTarget = Mathf.Atan2(
            toTarget.y,
            Mathf.Abs(toTarget.x)
        ) * Mathf.Rad2Deg;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}