
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Detection")]
    public float detectionRange = 1000f;
    public float attackDistance = 1.8f;
    public bool detectImmediately = true;

    [Header("Movement")]
    public float moveSpeed = 12f;
    public float acceleration = 100f;
    public float rotationSpeed = 720f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    [Header("Rush")]
    public bool rushImmediately = true;
    public float rushSpeed = 18f;

    [Header("Special Zombie")]
    public bool stunOnly = false;

    private PlayerHealth playerHealth;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private bool isDead;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("Zombie ต้องมี NavMeshAgent!", this);
            enabled = false;
            return;
        }

        // ตั้งค่าความเร็วและการเร่ง
        agent.speed = rushImmediately ? rushSpeed : moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = attackDistance * 0.8f;
        agent.autoBraking = false;
        agent.isStopped = false;

        FindPlayerTarget();

        if (player == null)
        {
            Debug.LogWarning(
                name + ": ไม่พบ Player กรุณาตรวจสอบ Tag Player", this);
        }
        else
        {
            Debug.Log(name + ": พบผู้เล่นแล้ว เริ่มไล่ทันที!");
        }
    }

    private void FindPlayerTarget()
    {
        if (player == null)
        {
            GameObject foundPlayer =
                GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player == null)
            return;

        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = player.GetComponentInChildren<PlayerHealth>();
    }

    private void Update()
    {
        if (isDead || agent == null || !agent.enabled)
            return;

        PlayerStun stun = GetComponent<PlayerStun>();

        if (stun != null && stun.IsStunned)
        {
            if (agent.isOnNavMesh)
                agent.isStopped = true;

            return;
        }

        if (player == null)
            FindPlayerTarget();

        if (player == null || !agent.isOnNavMesh)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // หันหน้าเข้าหาผู้เล่นทันที
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // ตรวจจับผู้เล่นทันที หรือเมื่ออยู่ในระยะ
        bool canChase =
            detectImmediately || distance <= detectionRange;

        if (!canChase)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        // หยุดเพื่อโจมตีเมื่ออยู่ใกล้ผู้เล่น
        if (distance <= attackDistance)
        {
            agent.isStopped = true;
            AttackPlayer();
            return;
        }

        // วิ่งไล่ผู้เล่นทันที
        agent.isStopped = false;
        agent.speed = rushImmediately ? rushSpeed : moveSpeed;

        // อัปเดตเป้าหมายทุกเฟรม เพื่อไล่ตามผู้เล่นที่เคลื่อนที่
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log(name + " โจมตีผู้เล่น!");
        }
    }

    public void StopAI()
    {
        isDead = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!stunOnly)
            return;

        bool isPlayer =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        PlayerStun stun = other.GetComponentInParent<PlayerStun>();

        if (stun != null)
        {
            stun.Stun();
            Debug.Log("Push Zombie stunned Player!");
        }
        else
        {
            Debug.LogWarning("PlayerStun component not found!");
        }
    }
}
