using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Boss : MonoBehaviour, IHit
{
    public int maxLife;
    public Slider hpBar;
    public Vector2 attackInterval;
    public GameObject bulletPrefab;
    public int attackCount;
    public float intervalAngle;
    public Transform attackTr;
    public static Boss instance;

    // 硬编码3个传送点（左、中、右平台）
    private readonly Vector3[] _teleportPoints = new Vector3[]
    {
        new Vector3(-7.0f, 1.14f, 0f),
        new Vector3(-0.5f, 1.14f, 0f),
        new Vector3(10.0f, 1.14f, 0f)
    };

    private int _nowLife;
    private Animator _animator;
    private float _nextAttackTime;
    private bool _isTeleporting;
    private bool _isEnraged;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _nowLife = maxLife;
        hpBar.value = 1;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        _nextAttackTime = Time.time + Random.Range(attackInterval.x, attackInterval.y);
    }

    private void Update()
    {
        if (_nowLife <= 0 || _isTeleporting)
            return;

        if (Time.time > _nextAttackTime)
        {
            _animator.SetTrigger("Fire");
            _nextAttackTime = Time.time + Random.Range(attackInterval.x, attackInterval.y);
        }
    }

    public void OnAttack()
    {
        Vector3 baseDir = Player.instance.transform.position - attackTr.position;
        baseDir.Normalize();

        float startAngle = intervalAngle * (int)(attackCount / 2) * -1;
        for (int j = 0; j < attackCount; j++)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, attackTr.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.moveDir = Quaternion.Euler(0, 0, startAngle + intervalAngle * j) * baseDir;
        }
    }

    public void Hit(int i, Vector2 dir)
    {
        if (_nowLife <= 0 || _isTeleporting)
            return;

        _nowLife -= i;
        if (_nowLife <= 0)
        {
            _nowLife = 0;
            GameManager.instance.GameVictory();
            return;
        }

        hpBar.value = (float)_nowLife / maxLife;

        // 半血狂暴（攻击更快 + 子弹更多 + 变红）
        if (!_isEnraged && _nowLife <= maxLife / 2)
        {
            _isEnraged = true;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.red;
            // 攻击间隔减半
            attackInterval = new Vector2(attackInterval.x * 0.5f, attackInterval.y * 0.5f);
            // 子弹数量 +3
            attackCount += 3;
            // 屏幕震动提示狂暴
            StartCoroutine(EnrageShake());
        }

        // 传送到随机其他平台
        if (_teleportPoints.Length > 1)
            StartCoroutine(TeleportToRandomPlatform());
    }

    private IEnumerator EnrageShake()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 origPos = cam.transform.position;
        float dur = 0.35f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float strength = (1f - t / dur) * 0.25f;
            cam.transform.position = origPos + (Vector3)(UnityEngine.Random.insideUnitCircle * strength);
            yield return null;
        }
        cam.transform.position = origPos;
    }

    private IEnumerator TeleportToRandomPlatform()
    {
        _isTeleporting = true;

        int currentIdx = GetNearestPointIndex();
        int newIdx;
        do { newIdx = Random.Range(0, _teleportPoints.Length); }
        while (newIdx == currentIdx);

        // 缩小消失
        float t = 0f;
        Vector3 baseScale = transform.localScale;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(1f, 0f, t / 0.15f);
            transform.localScale = baseScale * s;
            yield return null;
        }

        transform.position = _teleportPoints[newIdx];
        _nextAttackTime = Time.time + Random.Range(attackInterval.x, attackInterval.y);

        // 放大出现
        t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(0f, 1f, t / 0.15f);
            transform.localScale = baseScale * s;
            yield return null;
        }
        transform.localScale = baseScale;

        _isTeleporting = false;
    }

    private int GetNearestPointIndex()
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < _teleportPoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, _teleportPoints[i]);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_nowLife <= 0)
            return;
        if (col.transform.CompareTag("Player"))
        {
            IHit hit = col.gameObject.GetComponent<IHit>();
            if (hit != null)
                hit.Hit(1, col.transform.position - transform.position);
        }
    }
}
