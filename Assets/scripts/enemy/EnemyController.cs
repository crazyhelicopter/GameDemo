using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("怪物属性")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectRange = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("巡逻设置")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;
    private float attackTimer;

    private enum EnemyState { Idle, Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 如果没有设置巡逻点，自动生成两个点
        if (patrolPointA == null || patrolPointB == null)
        {
            SetupAutoPatrolPoints();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        // 状态切换逻辑
        if (distanceToPlayer <= detectRange && currentHealth > 0)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                currentState = EnemyState.Chase;
            }
        }
        else if (currentHealth > 0)
        {
            currentState = EnemyState.Patrol;
        }

        // 执行状态行为
        switch (currentState)
        {
            case EnemyState.Idle:
                // 啥也不做
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
    }

    void Patrol()
    {
        // 在两点之间来回移动（用Sin函数平滑移动）
        float t = Mathf.PingPong(Time.time * 0.5f, 1f);
        Vector2 targetPos = Vector2.Lerp(patrolPointA.position, patrolPointB.position, t);
        rb.velocity = (targetPos - (Vector2)transform.position).normalized * moveSpeed * 0.5f;
    }

    void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    void AttackPlayer()
    {
        rb.velocity = Vector2.zero; // 停止移动

        if (attackTimer <= 0)
        {
            // 攻击玩家
            PlayerController playerScript = player.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"怪物攻击玩家！造成{damage}伤害");
            }
            attackTimer = attackCooldown;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("怪物死亡！");
        // 触发死亡掉落或特效（如果有的话）
        // 启动重生协程，而不是直接销毁
        StartCoroutine(RespawnCoroutine());
    }

    private System.Collections.IEnumerator RespawnCoroutine()
    {
        // 1. 先隐藏起来（让玩家看不见，也碰不到）
        GetComponent<SpriteRenderer>().enabled = false; // 隐藏图像
        GetComponent<Collider2D>().enabled = false;     // 禁用碰撞，防止再被攻击

        // 2. 等待 3 秒（重生时间）
        yield return new WaitForSeconds(3f);

        // 3. 恢复显示和碰撞
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        // 4. 重置状态（满血复活）
        currentHealth = maxHealth;
        // 如果还有别的状态（比如巡逻位置），也在这里重置
        Debug.Log("怪物已重生！");
    }
    void SetupAutoPatrolPoints()
    {
        // 自动在怪物左右各3个单位生成巡逻点
        GameObject a = new GameObject("PatrolA");
        a.transform.position = (Vector2)transform.position + new Vector2(-3f, 0);
        patrolPointA = a.transform;

        GameObject b = new GameObject("PatrolB");
        b.transform.position = (Vector2)transform.position + new Vector2(3f, 0);
        patrolPointB = b.transform;
    }

    // 绘制调试信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
