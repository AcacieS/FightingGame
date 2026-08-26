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

    // Update is called once per frame
    void Update()
    {
        
    }
}
