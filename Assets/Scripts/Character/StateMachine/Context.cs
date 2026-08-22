using UnityEngine;

public class Context
{
    public Character Self { get; }
    public Character Target { get; }
    public bool IsLowHealth => SelfHp < 30;
    public bool TargetIsLowHealth => TargetHp < 30;
    public bool IsInAttackRange => Distance <= 2f;

    public float Distance { get; private set; }

    public float SelfHp => Self.Hp;
    public float SelfMaxHp => Self.Info.Hp;
    public float TargetHp => Target.Hp;
    public float TargetMaxHp => Target.Info.Hp;

    public bool TargetIsAttacking { get; set; }
    public bool TargetIsBlocking { get; set; }

    public Context(Character self, Character target)
    {
        Self = self;
        Target = target;
    }

    public void Update()
    {
        if (Self == null || Target == null)
            return;

        Distance = Vector3.Distance(
            Self.transform.position,
            Target.transform.position
        );
    }
}