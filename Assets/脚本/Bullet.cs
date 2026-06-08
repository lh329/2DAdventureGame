using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public Vector3 moveDir;
    public float moveSpeed;
    public bool targetIsPlayer;
    public int damage;
    public float lifeTime;
    
    private void Start()
    {
        targetIsPlayer = true;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
        transform.position += Time.deltaTime * moveSpeed * moveDir;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (targetIsPlayer)
        {
            if (col.CompareTag("Player"))
            {
                IHit hit = col.GetComponent<IHit>();
                hit.Hit(damage, Vector2.zero);
                Destroy(gameObject);
            }
        }
        else if(col.CompareTag("Enemy"))
        {
            IHit hit = col.GetComponent<IHit>();
            hit.Hit(damage, Vector2.zero);
            Destroy(gameObject);
        }
    }
}
