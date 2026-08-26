using UnityEngine;

public class GrabTryAttackState : ActionState
{
    [Header("Attack State")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask charactersLayer;

    [Header("Grab Attack State")]
    [SerializeField] private float _attackDistance;
    [SerializeField] private float _throwSpeed;
    [SerializeField] private float _throwAcceleration;
    [SerializeField] private float _retreatSpeed;
    [SerializeField] private float _retreatAcceleration;
    private float _speed;
    private float _acceleration;
    private Vector3 _startPosition;
    private float _direction;
    private Rigidbody2D rb;
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Grab Attack");
        _startPosition = attackPoint.position;
        rb = attackPoint.GetComponent<Rigidbody2D>();
        _direction = Context.DirectionSign;
        _speed = _throwSpeed;
        _acceleration = _throwAcceleration;
        isRetreat = false;
    }
    private bool isRetreat = false;
    public override void Play()
    {
        Attack();
        Move(_direction, _speed, _acceleration);
        float distanceMoved = Mathf.Abs(
            attackPoint.transform.position.x -
            _startPosition.x
        );
        if (distanceMoved >= _attackDistance)
        {
            RetreatGrab();
        }
        if (isRetreat && distanceMoved <= 0.1)
        {
            Move(0, 0, 0);
            attackPoint.position = _startPosition;
            RequestDecision();
        }
    }
    
    public void Move(float direction, float speed, float acceleration)
    {
        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D is NULL!");
            return;
        }

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
    }
    public override void Exit()
    {
        Debug.Log("AI → Exit Grab Attack");
    }
    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, charactersLayer);
        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
            //TODO: It just search for the same gameObject Character.
            if(enemy.GetComponent<Character>() == Context.Self) continue;
            StunPlayer();
            RetreatGrab();
            return;
        }
    }
    private void StunPlayer()
    {
        //Context.Target.Hurt(damage);
    }
    private void RetreatGrab()
    {
        if(isRetreat) return;
        _direction = -_direction;
        _speed = _retreatSpeed;
        _acceleration = _retreatAcceleration;
        isRetreat = true;
    }
    void OnDrawGizmosSelected()
    {
        if(attackPoint==null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}