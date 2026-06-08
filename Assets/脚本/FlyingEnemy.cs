using System.Collections;
using UnityEngine;

/// <summary>
/// 飞行小怪 FlyingEnemy
/// 完全参照 Enemy.cs 逻辑模式：巡逻 + 受击闪红 + 死亡销毁
/// 无俯冲攻击逻辑（和其他小怪一样简单可靠）
/// </summary>
public class FlyingEnemy : MonoBehaviour, IHit
{
    [Header("基础属性")]
    public int maxLife = 1;
    public int damage = 1;
    public float moveSpeed = 3f;

    [Header("巡逻路径（在Inspector里拖入空物体作为路径点）")]
    public Transform[] patrolPoints;

    [Header("受击反馈")]
    public float hitFlashTime = 0.06f;
    public Color hitFlashColor = new Color(1f, 0.6f, 0.6f, 1f);

    // ---- 私有状态 ----
    private Vector3[] _patrolPositions;
    private int _patrolIndex = 0;
    private int _nowLife;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Coroutine _hitFlashCoroutine;

    // ---- Unity 生命周期 ----
    private void Start()
    {
        _nowLife = maxLife;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            _patrolPositions = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
                _patrolPositions[i] = patrolPoints[i].position;
        }
        else
        {
            _patrolPositions = new Vector3[]
            {
                transform.position + Vector3.left * 2f,
                transform.position + Vector3.right * 2f
            };
        }
    }

    private void Update()
    {
        if (_nowLife <= 0)
            return;

        // 向当前巡逻目标飞
        Vector3 target = _patrolPositions[_patrolIndex];
        Vector3 dir = target - transform.position;
        Vector3 move = moveSpeed * Time.deltaTime * dir.normalized;

        if (dir.magnitude < 0.05f || move.magnitude >= dir.magnitude)
        {
            transform.position = target;
            _patrolIndex = (_patrolIndex + 1) % _patrolPositions.Length;
        }
        else
        {
            if (move.x > 0)
                _spriteRenderer.flipX = false;
            else if (move.x < 0)
                _spriteRenderer.flipX = true;

            transform.position += move;
        }
    }

    // ---- 碰撞伤害玩家 ----
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_nowLife <= 0) return;

        if (col.CompareTag("Player"))
        {
            IHit playerHit = col.GetComponent<IHit>();
            if (playerHit != null)
            {
                Vector2 knockback = (col.transform.position - transform.position).normalized;
                playerHit.Hit(damage, knockback);
            }
        }
    }

    // ---- 受击 ----
    public void Hit(int i, Vector2 dir)
    {
        if (_nowLife <= 0)
            return;

        _nowLife -= i;

        if (_nowLife <= 0)
        {
            ItemDrop.TryDropItem(transform.position);
            Destroy(gameObject);
        }
        else
        {
            _animator?.SetTrigger("Hit");
            TriggerHitFeedback();
        }
    }

    // ---- 受击闪烁 ----
    private void TriggerHitFeedback()
    {
        if (_spriteRenderer == null) return;
        if (_hitFlashCoroutine != null) StopCoroutine(_hitFlashCoroutine);
        _hitFlashCoroutine = StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        Color oldColor = _spriteRenderer.color;
        _spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        if (_spriteRenderer != null)
            _spriteRenderer.color = oldColor;
    }

    // ---- 编辑器辅助 ----
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
            if (i > 0 && patrolPoints[i - 1] != null)
                Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
        }
    }
}
