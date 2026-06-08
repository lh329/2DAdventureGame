using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IHit
{
    public float moveSpeed;
    public Transform footTr;
    public float checkR;
    public LayerMask groundLayer;
    public float jumpForce;
    public float hitForce;
    public RectTransform heartParentTr;
    public GameObject heartPrefab;
    public int MaxLife;

    [HideInInspector] 
    public bool canAttack = true;
    [HideInInspector]
    public float minX;
    [HideInInspector]
    public float maxX;
    [HideInInspector] 
    public bool LimitRightX = false;
    
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private bool _isGround;
    private int _nowLife;
    private List<Image> _heartLis;
    private bool _isLife = true;
    public static Player instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        
        _heartLis = new List<Image>();
        SetHeart(MaxLife);
    }

    public void Update()
    {
        _isGround = Physics2D.OverlapCircle(footTr.position, checkR, groundLayer);
        
        if (!_isLife)
        {
            return;
        }
        
        PlayerMove();
        PlayerJump();
        PlayerAttack();
    }

    private void LateUpdate()
    {
        if (transform.position.x < minX)
        {
            var position = transform.position;
            position = new Vector3(minX, position.y, 0);
            transform.position = position;
        }

        if (LimitRightX)
        {
            if (transform.position.x > maxX)
            {
                var position = transform.position;
                position = new Vector3(maxX, position.y, 0);
                transform.position = position;
            }
        }

        if (transform.position.y < -20)
        {
            Vector3 pos = transform.position;
            pos.y = 5;
            transform.position = pos;
            _rigidbody2D.velocity = Vector2.zero;
            SetHeart(-1);
        }
    }

    private void PlayerMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal > 0)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
        }else if (horizontal < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        _animator.SetBool("Move", Mathf.Abs(horizontal) > 0);
        
        if (canAttack)
        {
            // _rigidbody2D.velocity = new Vector2(horizontal * moveSpeed, _rigidbody2D.velocity.y);
            transform.position += horizontal * moveSpeed * Time.deltaTime * Vector3.right;
        }
        
    }

    private void PlayerJump()
    {
        _animator.SetFloat("Accelerated", _rigidbody2D.velocity.y);

        if (_isGround && Input.GetKeyDown(KeyCode.Space))
        {
            _rigidbody2D.velocity = Vector2.up * jumpForce + new Vector2(_rigidbody2D.velocity.x, 0);
        }
    }

    private void PlayerAttack()
    {
        if (canAttack && Input.GetKeyDown(KeyCode.Mouse0))
        {
            _animator.SetTrigger("Attack");
        }
    }

    public void SetHeart(int count)
    {
        if (!_isLife)
        {
            return;
        }
        
        _nowLife += count;
        if (_nowLife <= 0)
        {
            _nowLife = 0;
            _isLife = false;
            _animator.SetTrigger("Die");
            StartCoroutine(GameOver());
        }
        MaxLife = _nowLife > MaxLife ? _nowLife : MaxLife;
        for (int i = _heartLis.Count; i < MaxLife; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartParentTr);
            _heartLis.Add(heartObj.GetComponent<Image>());
        }

        for (int i = 0; i < MaxLife; i++)
        {
            _heartLis[i].color = i < _nowLife ? Color.white : Color.black;
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2);
        GameManager.instance.GameOver();
    }

    private bool _isInvincible;
    private bool _hasShield;
    private float _shieldEndTime;
    private SpriteRenderer _playerSprite;
    private Color _originalColor;
    private GameObject _shieldIcon;

    public void ActivateInvincible(float duration)
    {
        if (!_isLife) return;
        _isInvincible = true;
        if (_playerSprite == null)
        {
            _playerSprite = GetComponentInChildren<SpriteRenderer>();
            _originalColor = _playerSprite != null ? _playerSprite.color : Color.white;
        }
        StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (_playerSprite != null)
            {
                float blink = Mathf.PingPong(Time.time * 8f, 1f);
                _playerSprite.enabled = blink > 0.3f;
            }
            yield return null;
        }
        _isInvincible = false;
        if (_playerSprite != null)
            _playerSprite.enabled = true;
    }

    public void ActivateShield(float duration)
    {
        if (!_isLife) return;
        _hasShield = true;
        _shieldEndTime = Time.time + duration;

        // Get player sprite reference
        if (_playerSprite == null)
        {
            _playerSprite = GetComponentInChildren<SpriteRenderer>();
            _originalColor = _playerSprite != null ? _playerSprite.color : Color.white;
        }

        // Tint player blue-cyan to show shield active
        if (_playerSprite != null)
            _playerSprite.color = new Color(0.5f, 0.85f, 1f, 1f);

        // Small shield icon floating above player head
        if (_shieldIcon != null) Destroy(_shieldIcon);
        _shieldIcon = new GameObject("ShieldIcon");
        _shieldIcon.transform.SetParent(transform, false);
        _shieldIcon.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        SpriteRenderer iconSr = _shieldIcon.AddComponent<SpriteRenderer>();
        iconSr.sprite = CreateShieldIconSprite();
        iconSr.sortingLayerName = "Player";
        iconSr.sortingOrder = 20;
        iconSr.color = new Color(0.3f, 0.8f, 1f, 0.9f);
        _shieldIcon.transform.localScale = Vector3.one * 0.3f;

        StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        float t = 0f;
        while (t < duration && _hasShield)
        {
            t += Time.deltaTime;

            // Pulsing glow on player color
            if (_playerSprite != null && _hasShield)
            {
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                float r = Mathf.Lerp(0.4f, 0.6f, pulse);
                float g = Mathf.Lerp(0.75f, 0.95f, pulse);
                _playerSprite.color = new Color(r, g, 1f, 1f);
            }

            // Shield icon bobbing
            if (_shieldIcon != null)
            {
                float bob = Mathf.Sin(Time.time * 4f) * 0.05f;
                _shieldIcon.transform.localPosition = new Vector3(0f, 0.6f + bob, 0f);
            }

            yield return null;
        }

        _hasShield = false;

        // Restore original color
        if (_playerSprite != null)
            _playerSprite.color = _originalColor;

        // Remove icon
        if (_shieldIcon != null)
        {
            Destroy(_shieldIcon);
            _shieldIcon = null;
        }
    }

    /// <summary>
    /// Creates a small shield icon sprite (triangle shield shape)
    /// </summary>
    private static Sprite CreateShieldIconSprite()
    {
        int s = 32;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        // Draw a shield shape: pointed bottom, curved top
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float nx = (x + 0.5f) / s * 2f - 1f; // -1 to 1
                float ny = (y + 0.5f) / s * 2f - 1f; // -1 to 1

                float alpha = 0f;

                // Shield shape: wider at top, narrows to point at bottom
                float topWidth = 0.7f;
                float bottomPoint = -0.8f;
                float topEdge = 0.9f;

                if (ny > bottomPoint && ny < topEdge)
                {
                    // Width narrows as we go down
                    float t = (ny - bottomPoint) / (topEdge - bottomPoint);
                    float halfWidth = Mathf.Lerp(0.05f, topWidth, t);

                    if (Mathf.Abs(nx) < halfWidth)
                    {
                        // Inside shield
                        float edgeDist = halfWidth - Mathf.Abs(nx);

                        // Border zone
                        if (edgeDist < 0.12f || ny - bottomPoint < 0.12f || topEdge - ny < 0.08f)
                        {
                            alpha = 0.95f; // bright border
                        }
                        else
                        {
                            alpha = 0.5f; // semi-transparent fill
                        }
                    }
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
    }

    public void Hit(int i, Vector2 dir)
    {
        if (!_isLife) return;

        if (_isInvincible) return;
        if (_hasShield)
        {
            _hasShield = false;
            // Flash white on shield break
            if (_playerSprite != null)
                _playerSprite.color = Color.white;
            StartCoroutine(ShieldBreakFlash());
            return;
        }
        
        SetHeart(i * -1);
        _animator.SetTrigger("Hit");
        if (dir.sqrMagnitude > 0)
        {
            dir.Normalize();
        }
        _rigidbody2D.AddForce( dir * hitForce);

        // 受伤无敌帧0.8秒
        _isInvincible = true;
        if (_playerSprite == null)
        {
            _playerSprite = GetComponentInChildren<SpriteRenderer>();
            _originalColor = _playerSprite != null ? _playerSprite.color : Color.white;
        }
        StartCoroutine(HitInvincibleRoutine(0.8f));
    }

    private IEnumerator HitInvincibleRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (_playerSprite != null)
            {
                float blink = Mathf.PingPong(Time.time * 10f, 1f);
                _playerSprite.enabled = blink > 0.3f;
            }
            yield return null;
        }
        _isInvincible = false;
        if (_playerSprite != null)
            _playerSprite.enabled = true;
    }

    private IEnumerator ShieldBreakFlash()
    {
        // Brief white flash then restore color
        yield return new WaitForSeconds(0.15f);
        if (_playerSprite != null)
            _playerSprite.color = _originalColor;
        if (_shieldIcon != null)
        {
            Destroy(_shieldIcon);
            _shieldIcon = null;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(footTr.position, checkR);
    }
}
