using System.Collections;
using UnityEngine;

public class CooldownRequirement : Requirement
{
    [SerializeField] private float coolDownTime;

    [ReadOnly, SerializeField] private float time;

    private Coroutine cooldownCoroutine;

    public override void Initialize()
    {
        StartCooldown();
    }

    public override bool IsMet(Context context)
    {
        return time <= 0f;
    }

    public void StartCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine = StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        time = coolDownTime;

        while (time > 0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        time = 0f;
        cooldownCoroutine = null;
    }
}