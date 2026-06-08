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
            StartCoroutine(BossDeathSequence());
            return;
        }

        // 血条平滑过渡
        StartCoroutine(SmoothHpBar((float)_nowLife / maxLife));

        // 半血狂暴（攻击更快 + 子弹更多 + 变红 + 全屏红闪 + 震撼震动 + Boss膨胀）
        if (!_isEnraged && _nowLife <= maxLife / 2)
        {
            _isEnraged = true;
            // 暂停攻击，等狂暴演出结束
            _nextAttackTime = Time.time + 2.5f;
            StartCoroutine(EnrageSequence());
        }

        // 传送到随机其他平台
        if (_teleportPoints.Length > 1)
            StartCoroutine(TeleportToRandomPlatform());
    }

    private IEnumerator SmoothHpBar(float targetValue)
    {
        float startValue = hpBar.value;
        float t = 0f;
        float dur = 0.3f;
        while (t < dur)
        {
            t += Time.deltaTime;
            hpBar.value = Mathf.Lerp(startValue, targetValue, t / dur);
            yield return null;
        }
        hpBar.value = targetValue;
    }

    private IEnumerator BossDeathSequence()
    {
        Camera cam = Camera.main;
        Vector3 camOrigPos = cam != null ? cam.transform.position : Vector3.zero;
        Vector3 baseScale = transform.localScale;

        // 血条归零
        StartCoroutine(SmoothHpBar(0f));

        // === 阶段1：Boss闪烁+膨胀（1.0秒）===
        float t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime;
            // 白红交替闪烁，越来越快
            float blinkSpeed = 6f + t * 10f;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Mathf.PingPong(Time.time * blinkSpeed, 1f) > 0.5f ? Color.white : Color.red;
            // 缓慢膨胀
            float scale = 1f + t * 0.4f;
            transform.localScale = baseScale * scale;
            yield return null;
        }

        // === 阶段2：爆炸！慢动作+大震屏（1.5秒，时间缩放0.3）===
        Time.timeScale = 0.3f;

        // 创建爆炸闪光
        GameObject explosionObj = null;
        if (cam != null)
        {
            explosionObj = new GameObject("DeathExplosion");
            explosionObj.transform.SetParent(cam.transform, false);
            SpriteRenderer expSr = explosionObj.AddComponent<SpriteRenderer>();
            expSr.sortingOrder = 998;
            expSr.sortingLayerName = "Default";
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            expSr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            expSr.color = new Color(1f, 0.8f, 0.3f, 0.8f);
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;
            explosionObj.transform.localScale = new Vector3(camWidth * 2f, camHeight * 2f, 1f);
            explosionObj.transform.localPosition = new Vector3(0f, 0f, 10f);
        }

        // Boss碎裂效果：快速闪烁+缩放抖动
        t = 0f;
        float deathDur = 1.5f;
        while (t < deathDur)
        {
            t += Time.unscaledDeltaTime;
            float ratio = t / deathDur;

            // 震屏（强→弱）
            if (cam != null)
            {
                float strength = (1f - ratio) * 0.8f;
                cam.transform.position = camOrigPos + (Vector3)(Random.insideUnitCircle * strength);
            }

            // Boss抖动缩放
            float shake = 1f + 0.5f * Mathf.Sin(ratio * 40f) * (1f - ratio);
            transform.localScale = baseScale * (1f + ratio * 0.4f) * shake;

            // Boss越来越透明
            if (_spriteRenderer != null)
            {
                float blink = Mathf.PingPong(Time.unscaledTime * 15f, 1f) > 0.3f ? 1f : 0.3f;
                Color c = _spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, ratio * ratio) * blink;
                _spriteRenderer.color = c;
            }

            // 爆炸闪光渐隐
            if (explosionObj != null)
            {
                SpriteRenderer expSr = explosionObj.GetComponent<SpriteRenderer>();
                if (expSr != null)
                    expSr.color = new Color(1f, 0.8f, 0.3f, 0.8f * (1f - ratio));
            }

            yield return null;
        }

        // 隐藏Boss
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        // 隐藏魔毯子对象
        Transform carpet = transform.Find("MagicCarpet");
        if (carpet != null)
            carpet.gameObject.SetActive(false);

        // 恢复时间
        Time.timeScale = 1f;

        // 清理
        if (cam != null)
            cam.transform.position = camOrigPos;
        if (explosionObj != null)
            Destroy(explosionObj);

        // === 阶段3：短暂停顿后弹胜利面板 ===
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.GameVictory();
    }

    private IEnumerator EnrageSequence()
    {
        Camera cam = Camera.main;
        Vector3 camOrigPos = cam != null ? cam.transform.position : Vector3.zero;
        Vector3 baseScale = transform.localScale;

        // === 阶段1：蓄力停顿（0.6秒，Boss闪烁白光）===
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            // 白红交替闪烁
            if (_spriteRenderer != null)
                _spriteRenderer.color = Mathf.PingPong(t * 12f, 1f) > 0.5f ? Color.white : Color.red;
            yield return null;
        }

        // === 阶段2：爆发！全屏红闪 + 大震动 + Boss膨胀（1.0秒）===
        // 创建全屏红色闪光
        GameObject flashObj = null;
        if (cam != null)
        {
            flashObj = new GameObject("EnrageFlash");
            flashObj.transform.SetParent(cam.transform, false);
            SpriteRenderer flashSr = flashObj.AddComponent<SpriteRenderer>();
            flashSr.sortingOrder = 999;
            flashSr.sortingLayerName = "Default";
            // 全屏白色方块
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            flashSr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            flashSr.color = new Color(1f, 0f, 0f, 0.6f);
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;
            flashObj.transform.localScale = new Vector3(camWidth * 2f, camHeight * 2f, 1f);
            flashObj.transform.localPosition = new Vector3(0f, 0f, 10f);
        }

        // 大震动 + Boss膨胀
        t = 0f;
        float shakeDur = 1.0f;
        while (t < shakeDur)
        {
            t += Time.deltaTime;
            float ratio = t / shakeDur;

            // 震动（强→弱，幅度大）
            if (cam != null)
            {
                float strength = (1f - ratio) * 0.6f;
                cam.transform.position = camOrigPos + (Vector3)(Random.insideUnitCircle * strength);
            }

            // Boss膨胀到1.3倍再缩回
            float scalePulse = 1f + 0.3f * Mathf.Sin(ratio * Mathf.PI);
            transform.localScale = baseScale * scalePulse;

            // 红闪渐隐
            if (flashObj != null)
            {
                SpriteRenderer flashSr = flashObj.GetComponent<SpriteRenderer>();
                if (flashSr != null)
                    flashSr.color = new Color(1f, 0f, 0f, 0.6f * (1f - ratio));
            }

            yield return null;
        }

        // === 阶段3：余震 + 确认变红（0.8秒）===
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.red;
        transform.localScale = baseScale;

        t = 0f;
        float aftershockDur = 0.8f;
        while (t < aftershockDur)
        {
            t += Time.deltaTime;
            float ratio = t / aftershockDur;
            if (cam != null)
            {
                float strength = (1f - ratio) * 0.2f;
                cam.transform.position = camOrigPos + (Vector3)(Random.insideUnitCircle * strength);
            }
            // Boss微微红光脉动
            if (_spriteRenderer != null)
            {
                float pulse = 0.8f + 0.2f * Mathf.Sin(t * 10f);
                _spriteRenderer.color = new Color(1f, pulse * 0.1f, pulse * 0.1f);
            }
            yield return null;
        }

        // 清理
        if (cam != null)
            cam.transform.position = camOrigPos;
        if (flashObj != null)
            Destroy(flashObj);
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.red;

        // 正式进入狂暴状态
        attackInterval = new Vector2(attackInterval.x * 0.5f, attackInterval.y * 0.5f);
        attackCount += 3;
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
