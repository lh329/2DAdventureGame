using System;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public Transform attackTr;
    public Vector2 attackSize;
    public LayerMask attackLayer;
    public int damage;
    private void LateUpdate()
    {
        // attackColl.enabled = false;
    }

    public void OnAttack()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(attackTr.position, attackSize, 0, attackLayer);
        foreach (var coll in collider2Ds)
        {
            IHit hit = coll.GetComponent<IHit>();
            if (hit != null)
            {
                hit.Hit(damage, coll.transform.position - transform.position);
            }else if (coll.CompareTag("Bullet"))
            {
                Bullet bullet = coll.GetComponent<Bullet>();
                bullet.targetIsPlayer = false;
                if (Boss.instance)
                {
                    bullet.moveDir = (Boss.instance.transform.position - transform.position).normalized;
                }
                else
                {
                    bullet.moveDir *= -1;
                }
                
                bullet.moveSpeed *= 2;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackTr.position, attackSize);
    }
}
