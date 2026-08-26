using UnityEngine;
using System.Collections.Generic;

public class BulletHunterPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<BulletHunterWolf> availableBullets = new Queue<BulletHunterWolf>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateBullet();
        }
    }

    private BulletHunterWolf CreateBullet()
    {
        BulletHunterWolf bullet = Instantiate(bulletPrefab, transform).GetComponent<BulletHunterWolf>();

        bullet.gameObject.SetActive(false);

        availableBullets.Enqueue(bullet);

        return bullet;
    }

    public BulletHunterWolf GetBullet()
    {
        if (availableBullets.Count == 0)
        {
            return CreateBullet();
        }

        BulletHunterWolf bullet = availableBullets.Dequeue();

        bullet.gameObject.SetActive(true);

        return bullet;
    }

    public void ReturnBullet(BulletHunterWolf bullet)
    {
        bullet.gameObject.SetActive(false);

        availableBullets.Enqueue(bullet);
    }
}
