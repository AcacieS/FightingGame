using UnityEngine;

public class RollAttackState : MovementState
{
    [Header("Roll Attack State")]
    [SerializeField] private GameObject attackHitBox;

    public override void Enter()
    {
        Debug.Log("AI → Roll Attack");
        base.Enter();

        attackHitBox.SetActive(true);
    }

    public override void Play()
    {
        base.Play();
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Roll Attack");
        attackHitBox.SetActive(false);

        base.Exit();
    }
}