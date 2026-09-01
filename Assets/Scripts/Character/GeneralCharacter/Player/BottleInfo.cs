using System;
using UnityEngine;
[Serializable]
public class BottleInfo
{
    [Header("Range Attack")]
    [SerializeField] private int maxBottle = 3;
    [SerializeField] private string bottleAnim = "BottleAttack";

    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform spawnBottlePoint;

    [Header("Bottle Throw")]
    [SerializeField] private float bottleSpeed = 8f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private int bottleDamage = 10;
    [SerializeField] private LayerMask charactersLayer;
    [SerializeField] private LayerMask groundWallLayer;
    [SerializeField] private Audio bottleBreakSFX;
    [SerializeField] private Audio bottleHitSFX;
    [ReadOnly, SerializeField] private int nbBottle;

    public GameObject Prefab => bottlePrefab;
    public int MaxBottle => maxBottle;
    public string AnimName => bottleAnim;
    public int NbBottle => nbBottle;

    public System.Action<int> OnBottleChanged;

    public bool ThrowBottle()
    {
        if (nbBottle <= 0)
            return false;

        nbBottle--;

        OnBottleChanged?.Invoke(nbBottle);

        return true;
    }
    public void InitalizeBottle()
    {
        nbBottle = maxBottle;
        OnBottleChanged?.Invoke(nbBottle);
    }
    public void SpawnBottle(Character owner, float facingDirection)
    {
        if (bottlePrefab == null)
        {
            Debug.LogError("Bottle prefab is not assigned.");
            return;
        }

        if (spawnBottlePoint == null)
        {
            Debug.LogError("Spawn bottle point is not assigned.");
            return;
        }

        GameObject bottleObject = UnityEngine.Object.Instantiate(
            bottlePrefab,
            spawnBottlePoint.position,
            Quaternion.identity
        );

        Bottle bottle =
            bottleObject.GetComponent<Bottle>();

        if (bottle == null)
        {
            Debug.LogError(
                $"{bottleObject.name}: Bottle prefab does not have a Bottle component."
            );

            UnityEngine.Object.Destroy(bottleObject);
            return;
        }

        float radians = throwAngle * Mathf.Deg2Rad;

        Vector2 velocity = new Vector2(
            Mathf.Cos(radians) * facingDirection,
            Mathf.Sin(radians)
        ) * bottleSpeed;

        bottle.Initialize(
            owner,
            bottleDamage,
            velocity,
            charactersLayer,
            groundWallLayer,
            bottleHitSFX,
            bottleBreakSFX
        );
    }
}