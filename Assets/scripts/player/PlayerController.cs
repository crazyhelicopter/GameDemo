using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动属性")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;

    [Header("战斗属性")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("手机控制")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject attackBtn;
    [SerializeField] private GameObject dashBtn;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private int currentHealth;
    private float attackTimer;
    private bool isDashing;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int, int> OnHealthChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        bool isMobile = Application.isMobilePlatform;
        if (attackBtn != null) attackBtn.SetActive(isMobile);
        if (dashBtn != null) dashBtn.SetActive(isMobile);
    }

    void Update()
    {
        if (isDead) return;

        // ============ 获取移动输入（键盘 + 摇杆） ============
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 keyboardInput = new Vector2(h, v).normalized;

        Vector2 joystickInput = Vector2.zero;
        if (joystick != null)
        {
            joystickInput = joystick.GetInput();
        }

        // ⭐ 键盘优先：只有明确按住 WASD 时才用键盘
        if (keyboardInput.magnitude > 0.1f)
        {
            moveInput = keyboardInput;
        }
        else
        {
            moveInput = joystickInput;
        }

        if (moveInput.magnitude > 0.01f)
        {
            moveInput.Normalize();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // ============ 攻击冷却 ============
        attackTimer -= Time.deltaTime;

        // ============ 键盘攻击 (空格) ============
        if (Input.GetKeyDown(KeyCode.Space) && attackTimer <= 0)
        {
            Attack();
            attackTimer = attackCooldown;
        }

        // ============ 键盘冲刺 (左Shift) ============
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Revive();
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (!isDashing)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 2f);

        foreach (Collider2D col in hitEnemies)
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"攻击到敌人！造成{damage}伤害");
            }
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        rb.velocity = moveInput * dashSpeed;
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        Debug.Log("玩家死亡！");
        GameManager.Instance?.GameOver();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"恢复{amount}HP，当前血量：{currentHealth}");
    }

    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        transform.position = Vector3.zero;
        rb.velocity = Vector2.zero;
        Debug.Log("玩家已复活！");
    }

    public void OnAttackButtonPressed()
    {
        if (isDead) return;
        if (attackTimer <= 0)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    public void OnDashButtonPressed()
    {
        if (isDead) return;
        if (!isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}