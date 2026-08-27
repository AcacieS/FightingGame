using UnityEngine;

public class GrabAttackState : AttackState
{
    [Header("Attack")]
    [SerializeField] private GrabAttackObject attackPrefab;
    [SerializeField] private Transform attackSpawnPoint;

    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float attackDistance = 3f;

    [SerializeField] private LayerMask charactersLayer;

    [Header("Throw")]
    [SerializeField] private float throwSpeed = 8f;
    [SerializeField] private float throwAcceleration = 20f;

    [Header("Return")]
    [SerializeField] private float retreatSpeed = 8f;
    [SerializeField] private float retreatAcceleration = 20f;

    [ReadOnly, SerializeField]
    private GrabAttackObject currentAttack;

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Grab Attack");

        SpawnAttack();
    }

    private void SpawnAttack()
    {
        if (attackPrefab == null)
        {
            Debug.LogError(
                $"{name}: Attack Prefab is not assigned.",
                this
            );
            RequestDecision();
            return;
        }

        if (attackSpawnPoint == null)
        {
            Debug.LogError(
                $"{name}: Attack Spawn Point is not assigned.",
                this
            );
            RequestDecision();
            return;
        }

        float direction = Context.DirectionSign;

        currentAttack = Instantiate(
            attackPrefab,
            attackSpawnPoint.position,
            attackSpawnPoint.rotation
        );

        currentAttack.Initialize(
            Context.Self,
            Context.Target,
            direction,
            throwSpeed,
            throwAcceleration,
            retreatSpeed,
            retreatAcceleration,
            attackRange,
            attackDistance,
            damage,
            charactersLayer
        );
    }

    public override void Play()
    {
        if (currentAttack == null)
            return;

        if (currentAttack.HasReturned())
        {
            FinishAttack();
        }
    }

    private void FinishAttack()
    {
        Debug.Log("AI → Grab Attack Finished");
        attackResult = currentAttack.HasGrabPlayer? AttackResult.Success: 
            (currentAttack.PlayerHasBlocked? AttackResult.Blocked: 
            AttackResult.Miss);

        Destroy(currentAttack.gameObject);
        currentAttack = null;
        RequestDecision();
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Grab Attack");

        if (currentAttack != null)
        {
            Destroy(currentAttack.gameObject);
            currentAttack = null;
        }
    }
}