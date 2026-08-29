using UnityEngine;
using System.Collections.Generic;

public class TrapHunterPool : MonoBehaviour
{
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<TrapHunterWolf> availableTraps = new Queue<TrapHunterWolf>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateTrap();
        }
    }

    private TrapHunterWolf CreateTrap()
    {
        TrapHunterWolf Trap = Instantiate(trapPrefab, transform).GetComponent<TrapHunterWolf>();

        Trap.gameObject.SetActive(false);

        availableTraps.Enqueue(Trap);

        Trap.Initialize(this);

        return Trap;
    }

    public TrapHunterWolf GetTrap()
    {
        if (availableTraps.Count == 0)
        {
            return CreateTrap();
        }

        TrapHunterWolf Trap = availableTraps.Dequeue();

        Trap.gameObject.SetActive(true);
        Trap.transform.parent = null;

        return Trap;
    }

    public void ReturnTrap(TrapHunterWolf Trap)
    {
        Trap.gameObject.SetActive(false);

        Trap.transform.parent = transform;
        Trap.transform.position = transform.position;

        availableTraps.Enqueue(Trap);
    }
}
