
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerBullet : MonoBehaviour
{
    private Transform shooter;
    private Vector3 direction;

    private float speed;
    private float lifeTime;
    private int damage;
    private float bossDamageMultiplier;

    private float elapsed;
    private bool initialized;

    public void Initialize(
        Transform shooterTransform,
        Vector3 shootDirection,
        float bulletSpeed,
        float bulletLifeTime,
        int bulletDamage,
        float bossMultiplier)
    {
        shooter = shooterTransform;
        direction = shootDirection.normalized;
        speed = bulletSpeed;
        lifeTime = bulletLifeTime;
        damage = bulletDamage;
        bossDamageMultiplier = bossMultiplier;
        initialized = true;

        // ป้องกันกระสุนชนตัวผู้ยิง
        Collider[] shooterColliders =
            shooter.GetComponentsInChildren<Collider>();

        Collider bulletCollider = GetComponent<SphereCollider>();

        foreach (Collider shooterCollider in shooterColliders)
        {
            if (shooterCollider != null)
            {
                Physics.IgnoreCollision(
                    bulletCollider,
                    shooterCollider,
                    true
                );
            }
        }

        // ให้กระสุนหันไปตามทิศทางที่ยิง
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        if (!initialized)
            return;

        elapsed += Time.deltaTime;

        if (elapsed >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // เคลื่อนกระสุนไปข้างหน้า
        transform.position +=
            direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized || other == null)
            return;

        // ไม่ให้ทำดาเมจตัวเอง
        if (shooter != null &&
            (other.transform == shooter ||
             other.transform.IsChildOf(shooter)))
        {
            return;
        }

        // ตรวจสอบบอส
        BossHealth boss =
            other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            int bossDamage = Mathf.CeilToInt(
                damage * bossDamageMultiplier
            );

            boss.TakeDamage(bossDamage);

            Debug.Log(
                "[PlayerBullet] Hit Boss! Damage: " +
                bossDamage
            );

            Destroy(gameObject);
            return;
        }

        // ตรวจสอบซอมบี้
        ZombieHealth zombie =
            other.GetComponentInParent<ZombieHealth>();

        if (zombie != null)
        {
            zombie.TakeDamage(damage);

            Debug.Log(
                "[PlayerBullet] Hit Zombie! Damage: " +
                damage
            );

            Destroy(gameObject);
            return;
        }

        // ชนกำแพงหรือวัตถุอื่น ให้กระสุนหายไป
        Destroy(gameObject);
    }
}
