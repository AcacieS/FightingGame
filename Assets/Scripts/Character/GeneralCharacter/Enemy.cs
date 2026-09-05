using UnityEngine;

public class Enemy : Character
{
    [Header("Enemy")]
    [Tooltip("State Machine Controller: assign or by default from same gameObject")]
    [SerializeField] private AIController _aiController;
    public override void Awake()
    {
        base.Awake();
        if (_aiController == null)
        {
            _aiController = GetComponent<AIController>();
        }
    }
    public AIController AIController => _aiController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        
    }
    public override void Die()
    {
        if(IsDead) 
            return;
        
        base.Die();
        
        if(Context.Instance.Target is Player player){
            player.StopControl();
            player.PlayAnim("Ready");
        }
    }
    public override bool Hurt(int damage, bool isInterruptible = false, float stunDuration = 0f)
    {
        if (_aiController != null && _aiController.TryBlock())
        {
            Debug.Log($"{name} blocked the attack!");
            return false;
        }

        base.Hurt(damage, isInterruptible);
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
