using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("拾取属性")]
    [SerializeField] private PickupType type = PickupType.Health;
    [SerializeField] private int amount = 20;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatHeight = 0.3f;

    private Vector2 startPos;
    private float floatTimer;

    public enum PickupType { Health, Gold, Score }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 上下浮动动画
        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer) * floatHeight;
        transform.position = startPos + new Vector2(0, yOffset);

        // 旋转动画（如果是3D用，2D可以忽略）
        // transform.Rotate(0, 0, 30 * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            switch (type)
            {
                case PickupType.Health:
                    // 恢复血量（通过反射或公共方法，这里简单处理）
                    // 由于PlayerController没有公开恢复方法，我们通过事件模拟
                    // 更好的方式：在PlayerController加一个Heal方法
                    Debug.Log($"拾取血瓶 +{amount}HP");
                    // 直接调用（需要在PlayerController添加Heal方法）
                    player.SendMessage("Heal", amount, SendMessageOptions.DontRequireReceiver);
                    break;
                case PickupType.Gold:
                    Debug.Log($"拾取金币 +{amount}");
                    // 通知GameManager加金币
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddGold(amount);
                    }
                    break;
                    
            }
            StartCoroutine(RespawnCoroutine());
            // 拾取后销毁（附带简单特效，可以加一个粒子）
           
        }
    }
    private System.Collections.IEnumerator RespawnCoroutine()
    {
        // 1. 隐藏起来
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 2. 等待 5 秒（药瓶可以比怪物重生快一点）
        yield return new WaitForSeconds(5f);

        // 3. 重新出现
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
