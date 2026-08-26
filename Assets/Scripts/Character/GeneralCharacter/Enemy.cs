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
            if (_aiController == null)
            {
                Debug.LogError("AI CONTROLLER is not assigned or found in same object");
            }
        }
    }
    public AIController AIController => _aiController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        
    }
    public override void Hurt(int damage, bool isInterruptible = false)
    {
        if (_aiController != null && _aiController.TryBlock())
        {
            Debug.Log($"{name} blocked the attack!");
            return;
        }

        base.Hurt(damage, isInterruptible);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
