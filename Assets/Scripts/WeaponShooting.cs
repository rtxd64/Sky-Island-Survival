
using UnityEngine;

public class WeaponShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public Camera playerCamera;
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 0.25f;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public float bulletSpeed = 35f;
    public float bulletLifeTime = 3f;
    public float bulletSize = 0.2f;

    [Header("Boss Damage")]
    [Range(0f, 1f)]
    public float bossDamageMultiplier = 0.5f;

    private float nextFireTime;

    private void Update()
    {
        if (Input.GetMouseButton(0) &&
            Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning(
                "WeaponShooting: Player Camera is missing!"
            );
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                "WeaponShooting: Bullet Prefab is missing!"
            );
            return;
        }

        if (muzzlePoint == null)
        {
            Debug.LogWarning(
                "WeaponShooting: Muzzle Point is missing!"
            );
            return;
        }

        // หาจุดที่ต้องการยิงจากกลางหน้าจอ
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 aimPoint =
            ray.origin + ray.direction * range;

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            range,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        // คำนวณทิศทางจากปากกระบอกไปยังจุดเล็ง
        Vector3 direction =
            (aimPoint - muzzlePoint.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // สร้างกระสุนที่ปากกระบอกปืน
        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(direction)
        );

        bullet.transform.localScale =
            Vector3.one * bulletSize;

        // ตั้ง Collider ของกระสุน
        SphereCollider bulletCollider =
            bullet.GetComponent<SphereCollider>();

        if (bulletCollider == null)
        {
            bulletCollider =
                bullet.AddComponent<SphereCollider>();
        }

        bulletCollider.isTrigger = true;

        // เพิ่ม Rigidbody เพื่อให้ Trigger ตรวจจับการชนได้
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb == null)
            rb = bullet.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        // เพิ่มสคริปต์ควบคุมกระสุน
        PlayerBullet projectile =
            bullet.GetComponent<PlayerBullet>();

        if (projectile == null)
            projectile = bullet.AddComponent<PlayerBullet>();

        Transform shooter = playerCamera.transform.root;

        projectile.Initialize(
            shooter,
            direction,
            bulletSpeed,
            bulletLifeTime,
            damage,
            bossDamageMultiplier
        );

        Debug.Log("[Gun] Fired bullet!");
    }
}
