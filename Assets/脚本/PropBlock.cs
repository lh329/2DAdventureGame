using System;
using UnityEngine;

public class PropBlock : MonoBehaviour
{
    public int triggeringTimes = 1;
    public int integral;
    public GameObject goldCoin;
    private bool isTriggered;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !isTriggered)
        {
            _animator.Play("道具方块");
            isTriggered = --triggeringTimes <= 0;
            Instantiate(goldCoin, transform.position, Quaternion.identity);
            GameManager.instance.AddIntegral(integral);
            _animator.SetBool("isTriggered", isTriggered);
        }
    }
}
