using System;
using UnityEngine;


public class Enemy : MonoBehaviour, IHit
{
    public int maxLife;
    public int damage;
    public float hitTime;
    public Transform[] moveTrs;
    public float moveSpeed; // 移动速度
    public float topAngle = 30;
    
    private Vector3[] _moveVector3S;
    private int index; // 当前的目标点
    private SpriteRenderer _spriteRenderer;
    private int _nowLife;
    private float _hitWaitTime;
    private Animator _animator;
    
    private void Start()
    {
        index = 0;
        _nowLife = maxLife;
        _moveVector3S = new Vector3[moveTrs.Length];
        for (int i = 0; i < moveTrs.Length; i++)
        {
            _moveVector3S[i] = moveTrs[i].position;
        }
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    // 移动
    private void Update()
    {
        if (_nowLife <= 0 || _hitWaitTime > Time.time)
        {
            return;
        }
        
        Vector3 targetPos = _moveVector3S[index];
        Vector3 direction = targetPos - transform.position;
        Vector3 moveDis = moveSpeed * Time.deltaTime * direction.normalized;
        if (direction.magnitude < 0.05f || moveDis.magnitude >= direction.magnitude)
        {
            transform.position = targetPos;
            index++;

            if (index >= _moveVector3S.Length)
            {
                Array.Reverse(_moveVector3S);
                index = 0;
            }
        }
        else
        {
            if (moveDis.x > 0)
            {
                _spriteRenderer.flipX = false;
            }else if (moveDis.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
            transform.position += moveDis;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_nowLife <= 0)
        {
            return;
        }

        Vector2 dir = col.transform.position - transform.position;
        float angle = Vector2.Angle(Vector2.up, dir);
        if (col.CompareTag("Player"))
        {
            if (angle < topAngle)
            {
                Hit(1, Vector2.zero);
                col.GetComponent<Rigidbody2D>().velocity = dir.normalized * 10;
            }
            else
            {
                IHit hit = col.GetComponent<IHit>();
                if (hit != null)
                {
                    hit.Hit(damage, col.transform.position - transform.position);
                }
            }
        }
        
    }

    public void Hit(int i, Vector2 dir)
    {
        if (_nowLife <= 0)
        {
            return;
        }
        
        if (dir.sqrMagnitude > 0)
        {
            dir.Normalize();
        }
        _nowLife -= i;
        _hitWaitTime = Time.time + hitTime;
        if (_nowLife <= 0)
        {
            ItemDrop.TryDropItem(transform.position);
            Destroy(gameObject);
        }
        else
        {
            _animator.SetTrigger("Hit");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0,0, topAngle) * Vector3.up);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0,0, -topAngle) * Vector3.up);
    }
}
