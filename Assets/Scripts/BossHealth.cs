
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 1250;
    public int currentHealth = 1250;
    public BossFightManager manager;
    public Transform player;

    [Header("Phase 1: HP >= 600")]
    public int redZonesBeforeLaser = 4;
    public float warningTime = 1.5f;
    public float redZoneRadius = 5f;
    public float redZoneDuration = 4f;
    public float redZoneDamage = 12f;
    public float redZoneTick = 0.5f;

    [Header("Red Zone Height")]
    public float redZoneHeightOffset = 0.03f;

    [Header("Big Red Laser")]
    public float laserWarningTime = 2f;
    public float laserDuration = 5f;
    public float laserLength = 40f;
    public float laserWidth = 4f;
    public float laserHeight = 1.2f;
    public float laserDamage = 12f;
    public float laserTick = 0.5f;
    public bool lockLaserToPlayer = true;
    public float laserTrackingSpeed = 360f;

    [Header("Phase 2: Stationary Laser Orb")]
    public float orbShootInterval = 2.5f;
    public float orbShootWarning = 0.75f;
    public float orbSpeed = 8f;
    public float orbLifeTime = 6f;
    public float orbSize = 1.5f;
    public float orbDamage = 5f;
    public float orbDamageInterval = 0.5f;
    public float orbSpawnHeight = 1.5f;
    public float orbHitRadius = 1.5f;
    public Color orbColor = Color.red;

    [Header("Phase 2 Movement")]
    public bool stopCompletelyInPhaseTwo = true;
    public float turnSpeed = 360f;
    [Header("Boss Knockback Resistance")]
    public bool preventKnockback = true;
    public bool freezeRigidbodyInPhaseTwo = true;

    private Rigidbody bossRigidbody;

    [Header("Boss Health Bar")]
    public float healthBarHeight = 2f;
    public float healthBarWidth = 200f;
    public float healthBarDistanceLimit = 60f;

    private NavMeshAgent agent;
    private bool dead;
    private bool attacking;
    private bool phaseTwo;
    private int zonesDone;
    private Coroutine phaseOneRoutine;
    private Coroutine phaseTwoRoutine;
    private GameObject laserObject;
    private readonly List<GameObject> activeOrbs =
        new List<GameObject>();

    private void Awake()
    {
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        bossRigidbody = GetComponent<Rigidbody>();

        if (bossRigidbody != null && preventKnockback)
        {
            // ลดผลกระทบจากแรงผลักใน Phase 1
            bossRigidbody.mass = 10000f;
            bossRigidbody.linearDamping = 10f;
            bossRigidbody.angularDamping = 10f;

            // ป้องกันบอสล้มเอียง
            bossRigidbody.constraints |=
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Start()
    {
        FindPlayer();

        if (manager == null)
            manager = BossFightManager.Instance;
    }

    private void FindPlayer()
    {
        if (player != null && GetPlayerHealth() != null)
            return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    private void Update()
    {
        if (dead)
            return;

        if (player == null)
            FindPlayer();

        if (player == null)
            return;

        if (manager != null && !manager.IsBossFightActive)
            return;

        // เข้าสู่เฟส 2 เมื่อ HP ต่ำกว่า 600
        if (currentHealth < 600 && !phaseTwo)
        {
            EnterPhaseTwo();
        }

        if (phaseTwo)
        {
            FacePlayer();

            if (phaseTwoRoutine == null && !dead)
                phaseTwoRoutine = StartCoroutine(PhaseTwoLoop());
        }
        else if (!attacking && phaseOneRoutine == null)
        {
            phaseOneRoutine = StartCoroutine(PhaseOneLoop());
        }
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
            Die();
    }

    private void EnterPhaseTwo()
    {
        phaseTwo = true;

        Debug.Log("BOSS ENTERED PHASE 2: STATIONARY LASER ORB");

        // หยุดการโจมตี Phase 1
        if (phaseOneRoutine != null)
        {
            StopCoroutine(phaseOneRoutine);
            phaseOneRoutine = null;
        }

        attacking = false;

        // ลบเลเซอร์เก่า
        if (laserObject != null)
        {
            Destroy(laserObject);
            laserObject = null;
        }

        // หยุด NavMeshAgent เพื่อไม่ให้บอสเดิน
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.enabled = false;
        }

        // ป้องกันบอสถูกผลักใน Phase 2
        if (bossRigidbody != null && freezeRigidbodyInPhaseTwo)
        {
            bossRigidbody.linearVelocity = Vector3.zero;
            bossRigidbody.angularVelocity = Vector3.zero;
            bossRigidbody.isKinematic = true;
        }

        if (manager != null)
            manager.ShowMessage("BOSS PHASE 2!", 2f);
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    private IEnumerator PhaseOneLoop()
    {
        attacking = true;

        if (zonesDone < redZonesBeforeLaser)
        {
            yield return Warning("RED ZONE INCOMING!");

            if (!dead && !phaseTwo)
            {
                yield return RedZoneAttack();
                zonesDone++;
            }
        }
        else
        {
            if (manager != null)
                manager.ShowMessage(
                    "BOSS BIG LASER INCOMING!",
                    laserWarningTime);

            yield return new WaitForSeconds(laserWarningTime);

            if (!dead && !phaseTwo)
                yield return LaserAttack();

            zonesDone = 0;
        }

        yield return new WaitForSeconds(2f);

        attacking = false;
        phaseOneRoutine = null;
    }

    private IEnumerator Warning(string message)
    {
        if (manager != null)
            manager.ShowMessage(message, warningTime);

        yield return new WaitForSeconds(warningTime);
    }

    private PlayerHealth GetPlayerHealth()
    {
        if (player != null)
        {
            PlayerHealth ph =
                player.GetComponentInParent<PlayerHealth>();

            if (ph == null)
                ph = player.GetComponentInChildren<PlayerHealth>();

            if (ph != null)
                return ph;
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            PlayerHealth ph =
                p.GetComponentInParent<PlayerHealth>();

            if (ph == null)
                ph = p.GetComponentInChildren<PlayerHealth>();

            if (ph != null)
            {
                player = p.transform;
                return ph;
            }
        }

        PlayerHealth[] allPlayers =
            FindObjectsByType<PlayerHealth>(
                FindObjectsSortMode.None);

        foreach (PlayerHealth ph in allPlayers)
        {
            if (ph.gameObject.CompareTag("Player") ||
                ph.transform.root.CompareTag("Player"))
            {
                player = ph.transform;
                return ph;
            }
        }

        return null;
    }

    private IEnumerator RedZoneAttack()
    {
        Vector3 pos = player != null
            ? player.position
            : transform.position;

        pos.y += redZoneHeightOffset;

        GameObject zone =
            new GameObject("Boss Red Damage Zone");

        zone.transform.position = pos;

        GameObject disc =
            GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        disc.name = "Red Warning Circle";
        disc.transform.SetParent(zone.transform, false);
        disc.transform.localPosition = Vector3.zero;
        disc.transform.localScale = new Vector3(
            redZoneRadius * 2f,
            0.025f,
            redZoneRadius * 2f);

        Collider col = disc.GetComponent<Collider>();

        if (col != null)
            Destroy(col);

        Renderer rend = disc.GetComponent<Renderer>();

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            rend.material = new Material(shader);
            rend.material.color =
                new Color(1f, 0f, 0f, 0.45f);
        }

        float elapsed = 0f;
        float tick = 0f;

        while (elapsed < redZoneDuration &&
               !dead && !phaseTwo)
        {
            PlayerHealth ph = GetPlayerHealth();

            if (ph != null)
            {
                Vector3 target = ph.transform.position;

                Vector2 playerXZ =
                    new Vector2(target.x, target.z);

                Vector2 zoneXZ =
                    new Vector2(pos.x, pos.z);

                if (Vector2.Distance(playerXZ, zoneXZ)
                    <= redZoneRadius)
                {
                    tick += Time.deltaTime;

                    if (tick >= redZoneTick)
                    {
                        ph.TakeDamage(
                            Mathf.CeilToInt(redZoneDamage));

                        tick = 0f;
                    }
                }
                else
                {
                    tick = 0f;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (zone != null)
            Destroy(zone);
    }

    private IEnumerator LaserAttack()
    {
        if (laserObject != null)
            Destroy(laserObject);

        laserObject = new GameObject("Boss Big Red Laser");

        LineRenderer line =
            laserObject.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = laserWidth;
        line.endWidth = laserWidth;
        line.numCapVertices = 8;
        line.numCornerVertices = 8;
        line.alignment = LineAlignment.View;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            line.material = new Material(shader);
            line.material.color = Color.red;
        }

        line.startColor = Color.red;
        line.endColor = Color.red;

        float currentAngle = transform.eulerAngles.y;
        float elapsed = 0f;
        float damageTimer = 0f;

        while (elapsed < laserDuration &&
               !dead && !phaseTwo)
        {
            PlayerHealth ph = GetPlayerHealth();

            Vector3 center =
                transform.position +
                Vector3.up * laserHeight;

            if (lockLaserToPlayer && ph != null)
            {
                Vector3 target = ph.transform.position;
                target.y = center.y;

                Vector3 toPlayer = target - center;

                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    float targetAngle =
                        Mathf.Atan2(toPlayer.x, toPlayer.z)
                        * Mathf.Rad2Deg;

                    currentAngle = Mathf.MoveTowardsAngle(
                        currentAngle,
                        targetAngle,
                        laserTrackingSpeed * Time.deltaTime);
                }
            }

            Vector3 direction =
                Quaternion.Euler(0f, currentAngle, 0f)
                * Vector3.forward;

            Vector3 end = center + direction * laserLength;

            line.SetPosition(0, center);
            line.SetPosition(1, end);

            bool playerHit = false;
            Vector3 targetPosition = Vector3.zero;

            if (ph != null)
            {
                targetPosition = ph.transform.position;

                Collider[] colliders =
                    ph.GetComponentsInChildren<Collider>();

                float closestColliderDistance = float.MaxValue;

                foreach (Collider playerCollider in colliders)
                {
                    if (playerCollider == null ||
                        !playerCollider.enabled)
                        continue;

                    Vector3 candidate =
                        playerCollider.bounds.center;

                    Vector2 candidateXZ =
                        new Vector2(candidate.x, candidate.z);

                    Vector2 startXZ =
                        new Vector2(center.x, center.z);

                    Vector2 endXZ =
                        new Vector2(end.x, end.z);

                    Vector2 segment = endXZ - startXZ;

                    float t = segment.sqrMagnitude > 0.001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(
                                candidateXZ - startXZ,
                                segment) / segment.sqrMagnitude)
                        : 0f;

                    Vector2 nearest = startXZ + segment * t;

                    float candidateDistance =
                        Vector2.Distance(candidateXZ, nearest);

                    if (candidateDistance < closestColliderDistance)
                    {
                        closestColliderDistance = candidateDistance;
                        targetPosition = candidate;
                    }
                }

                Vector2 playerXZ =
                    new Vector2(targetPosition.x, targetPosition.z);

                Vector2 laserStartXZ =
                    new Vector2(center.x, center.z);

                Vector2 laserEndXZ =
                    new Vector2(end.x, end.z);

                Vector2 laserSegment =
                    laserEndXZ - laserStartXZ;

                float laserT = laserSegment.sqrMagnitude > 0.001f
                    ? Mathf.Clamp01(
                        Vector2.Dot(
                            playerXZ - laserStartXZ,
                            laserSegment) /
                        laserSegment.sqrMagnitude)
                    : 0f;

                Vector2 nearestPoint =
                    laserStartXZ + laserSegment * laserT;

                float hitDistance =
                    Vector2.Distance(playerXZ, nearestPoint);

                if (hitDistance <= laserWidth * 0.5f + 0.5f)
                    playerHit = true;
            }

            if (playerHit && ph != null)
            {
                damageTimer += Time.deltaTime;

                if (damageTimer >= laserTick)
                {
                    ph.TakeDamage(
                        Mathf.CeilToInt(laserDamage));

                    damageTimer = 0f;
                }
            }
            else
            {
                damageTimer = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (laserObject != null)
            Destroy(laserObject);

        laserObject = null;
    }

    private IEnumerator PhaseTwoLoop()
    {
        while (!dead && phaseTwo)
        {
            FacePlayer();

            if (manager != null)
                manager.ShowMessage(
                    "BOSS LASER ORB INCOMING!",
                    orbShootWarning);

            yield return new WaitForSeconds(orbShootWarning);

            if (dead || !phaseTwo)
                break;

            ShootLaserOrb();

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, orbShootInterval));
        }

        phaseTwoRoutine = null;
    }

    private void ShootLaserOrb()
    {
        if (player == null || dead)
            return;

        Vector3 spawnPosition =
            transform.position +
            Vector3.up * orbSpawnHeight +
            transform.forward * 1.5f;

        GameObject orb =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        orb.name = "Boss Laser Orb";
        orb.transform.position = spawnPosition;
        orb.transform.localScale = Vector3.one * orbSize;

        Renderer rend = orb.GetComponent<Renderer>();

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = orbColor;
            rend.material = mat;
        }

        Collider orbCollider = orb.GetComponent<Collider>();

        if (orbCollider != null)
            orbCollider.isTrigger = true;

        Rigidbody rb = orb.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        LaserOrbProjectile projectile =
            orb.AddComponent<LaserOrbProjectile>();

        projectile.Initialize(
            player,
            orbSpeed,
            orbLifeTime,
            Mathf.CeilToInt(orbDamage),
            orbDamageInterval,
            orbHitRadius);

        activeOrbs.Add(orb);

        Debug.Log("Boss fired a laser orb.");
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        if (phaseOneRoutine != null)
            StopCoroutine(phaseOneRoutine);

        if (phaseTwoRoutine != null)
            StopCoroutine(phaseTwoRoutine);

        if (laserObject != null)
            Destroy(laserObject);

        foreach (GameObject orb in activeOrbs)
        {
            if (orb != null)
                Destroy(orb);
        }

        activeOrbs.Clear();

        if (manager != null)
            manager.BossDefeated();

        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (dead || Camera.main == null)
            return;

        Vector3 worldPosition =
            transform.position + Vector3.up * healthBarHeight;

        Vector3 screen =
            Camera.main.WorldToScreenPoint(worldPosition);

        if (screen.z <= 0f ||
            screen.z > healthBarDistanceLimit)
            return;

        float x = screen.x - healthBarWidth / 2f;
        float y = Screen.height - screen.y;

        GUI.color = Color.black;
        GUI.DrawTexture(
            new Rect(x, y, healthBarWidth, 18f),
            Texture2D.whiteTexture);

        GUI.color = Color.red;

        GUI.DrawTexture(
            new Rect(
                x, y,
                healthBarWidth * currentHealth /
                Mathf.Max(1, maxHealth),
                18f),
            Texture2D.whiteTexture);

        GUI.color = Color.white;

        GUI.Label(
            new Rect(x, y - 24f, healthBarWidth, 22f),
            "BOSS " + currentHealth + " / " + maxHealth);
    }
}

// Component สำหรับบอลเลเซอร์ของบอส


public class LaserOrbProjectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float lifeTime;
    private int damage;
    private float damageInterval;
    private float hitRadius;

    private Vector3 direction;
    private float elapsed;
    private float damageTimer;

    private PlayerHealth damagedPlayer;

    public void Initialize(
        Transform targetTransform,
        float moveSpeed,
        float lifetime,
        int damageAmount,
        float tickInterval,
        float radius)
    {
        target = targetTransform;
        speed = moveSpeed;
        lifeTime = lifetime;
        damage = damageAmount;
        damageInterval = Mathf.Max(0.05f, tickInterval);
        hitRadius = Mathf.Max(0.1f, radius);

        if (target != null)
        {
            direction =
                (target.position - transform.position).normalized;
        }

        Debug.Log("[LaserOrb] Initialized. Damage = " + damage);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 previousPosition = transform.position;

        // เคลื่อนบอลไปตามทิศทางที่เล็งไว้ตอนยิง
        transform.position +=
            direction * speed * Time.deltaTime;

        Vector3 movement =
            transform.position - previousPosition;

        // ตรวจทั้งตำแหน่งปัจจุบันและเส้นทางที่บอลเคลื่อนผ่าน
        float radius = hitRadius + movement.magnitude * 0.5f;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            ~0,
            QueryTriggerInteraction.Collide);

        PlayerHealth foundPlayer = null;

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            PlayerHealth health =
                hit.GetComponentInParent<PlayerHealth>();

            if (health == null)
                health = hit.GetComponentInChildren<PlayerHealth>();

            if (health != null && health.currentHealth > 0)
            {
                foundPlayer = health;
                break;
            }
        }

        if (foundPlayer != null)
        {
            if (damagedPlayer != foundPlayer)
            {
                damagedPlayer = foundPlayer;
                damageTimer = damageInterval;
            }

            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                Debug.Log(
                    "[LaserOrb] HIT! Applying damage: " + damage);

                foundPlayer.TakeDamage(damage);

                damageTimer = 0f;
            }
        }
        else
        {
            damagedPlayer = null;
            damageTimer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
