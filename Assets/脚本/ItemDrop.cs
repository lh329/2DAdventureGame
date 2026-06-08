using System.Collections;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public enum ItemType { Heart, Invincible, Shield }

    [Header("Settings")]
    public ItemType itemType;
    public float lifetime = 8f;
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.12f;
    public float dropBounceForce = 3f;
    public float invincibleDuration = 4f;
    public float shieldDuration = 8f;

    private float _spawnTime;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2D;
    private bool _grounded;
    private bool _collected;

    private static Sprite _heartSprite;
    private static Sprite _invincibleSprite;
    private static Sprite _shieldSprite;

    // ---- 掉落入口（静态工厂）----
    public static void TryDropItem(Vector3 position)
    {
        float r = UnityEngine.Random.value;
        ItemType type;
        if      (r < 0.30f) type = ItemType.Heart;       // 30% 心
        else if (r < 0.50f) type = ItemType.Invincible;  // 20% 无敌
        else if (r < 0.70f) type = ItemType.Shield;      // 20% 护盾
        else return;                                       // 30% 不掉

        GameObject obj = new GameObject("ItemDrop_" + type);
        obj.transform.position = position;

        // Rigidbody2D
        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.8f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 物理碰撞器（非Trigger）用于落地
        CircleCollider2D phyCol = obj.AddComponent<CircleCollider2D>();
        phyCol.isTrigger = false;
        phyCol.radius = 0.22f;

        // SpriteRenderer
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        // ItemDrop 组件
        ItemDrop drop = obj.AddComponent<ItemDrop>();
        drop.itemType = type;
        drop._spawnTime = Time.time;
        drop._spriteRenderer = sr;
        drop._rigidbody2D = rb;

        // 赋精灵
        switch (type)
        {
            case ItemType.Heart:
                EnsureHeartSprite();
                sr.sprite = _heartSprite;
                sr.color = new Color(1f, 0.35f, 0.35f);
                break;
            case ItemType.Invincible:
                EnsureInvincibleSprite();
                sr.sprite = _invincibleSprite;
                sr.color = new Color(1f, 0.85f, 0.15f);
                break;
            case ItemType.Shield:
                EnsureShieldSprite();
                sr.sprite = _shieldSprite;
                sr.color = new Color(0.3f, 0.65f, 1f);
                break;
        }

        obj.transform.localScale = Vector3.one * 0.55f;

        // 拾取 Trigger（子对象）
        GameObject pickupZone = new GameObject("PickupZone");
        pickupZone.transform.SetParent(obj.transform, false);
        CircleCollider2D pickupCol = pickupZone.AddComponent<CircleCollider2D>();
        pickupCol.isTrigger = true;
        pickupCol.radius = 0.45f;
        PickupTrigger pt = pickupZone.AddComponent<PickupTrigger>();
        pt.owner = drop;

        // 初速度（弹起）
        rb.velocity = new Vector2(UnityEngine.Random.Range(-1.2f, 1.2f), drop.dropBounceForce);

        drop.StartCoroutine(drop.LifetimeRoutine());
    }

    // ---- Update：落地后浮动 ----
    private void Update()
    {
        if (_collected) return;

        if (_grounded && _rigidbody2D != null)
        {
            // 停止水平漂移
            _rigidbody2D.velocity = new Vector2(0f, _rigidbody2D.velocity.y);

            // 上下浮动（仅Y偏移，不动Rigidbody，避免穿透）
            float t = Time.time - _spawnTime;
            Vector3 pos = transform.position;
            pos.y += Mathf.Cos(t * floatSpeed) * floatAmplitude * Time.deltaTime;
            transform.position = pos;
        }
    }

    // ---- 落地检测 ----
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_collected || _grounded) return;
        // 确认是从上方落到平台
        foreach (ContactPoint2D cp in col.contacts)
        {
            if (cp.normal.y > 0.5f)
            {
                _grounded = true;
                if (_rigidbody2D != null)
                {
                    _rigidbody2D.velocity = Vector2.zero;
                    _rigidbody2D.gravityScale = 0f;
                }
                break;
            }
        }
    }

    // ---- 拾取（由PickupTrigger调用）----
    public void OnPlayerPickup()
    {
        if (_collected) return;
        _collected = true;
        ApplyEffect();
        StartCoroutine(CollectAnim());
    }

    private void ApplyEffect()
    {
        if (Player.instance == null) return;
        switch (itemType)
        {
            case ItemType.Heart:
                Player.instance.SetHeart(1);
                break;
            case ItemType.Invincible:
                Player.instance.ActivateInvincible(invincibleDuration);
                break;
            case ItemType.Shield:
                Player.instance.ActivateShield(shieldDuration);
                break;
        }
    }

    // ---- 生命周期 ----
    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (!_collected) StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (_spriteRenderer == null) { Destroy(gameObject); yield break; }
        float t = 0f;
        float dur = 0.6f;
        Color c = _spriteRenderer.color;
        while (t < dur && !_collected)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - t / dur);
            _spriteRenderer.color = c;
            yield return null;
        }
        if (!_collected) Destroy(gameObject);
    }

    private IEnumerator CollectAnim()
    {
        float t = 0f;
        float dur = 0.18f;
        Vector3 startScale = transform.localScale;
        Color c = _spriteRenderer != null ? _spriteRenderer.color : Color.white;
        while (t < dur)
        {
            t += Time.deltaTime;
            float ratio = t / dur;
            transform.localScale = startScale * (1f + ratio * 0.8f);
            if (_spriteRenderer != null)
            {
                c.a = 1f - ratio;
                _spriteRenderer.color = c;
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    // ---- 精灵生成 ----

    private static void EnsureHeartSprite()
    {
        if (_heartSprite != null) return;
        int s = 32;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                // 归一化到 [-1,1]
                float nx = (x + 0.5f) / s * 2f - 1f;
                float ny = (y + 0.5f) / s * 2f - 1f;
                // 心形公式（x²+y²-1)³ - x²y³ < 0
                float val = HeartSDF(nx, ny - 0.1f);
                float alpha = Mathf.Clamp01(-val * 6f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _heartSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
    }

    private static float HeartSDF(float x, float y)
    {
        // 翻转y，心尖朝下
        y = -y;
        float a = x * x + y * y - 1f;
        return a * a * a - x * x * y * y * y;
    }

    private static void EnsureInvincibleSprite()
    {
        if (_invincibleSprite != null) return;
        int s = 32;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float cx = (x + 0.5f) / s * 2f - 1f;
                float cy = (y + 0.5f) / s * 2f - 1f;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);
                float angle = Mathf.Atan2(cy, cx);
                // 5角星：极坐标方程
                float star = StarSDF(cx, cy, 5, 0.85f, 0.4f);
                float alpha = Mathf.Clamp01(-star * 8f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _invincibleSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
    }

    private static float StarSDF(float x, float y, int n, float outerR, float innerR)
    {
        float angle = Mathf.Atan2(y, x);
        float dist = Mathf.Sqrt(x * x + y * y);
        float step = Mathf.PI / n;
        float a = Mathf.Repeat(angle + Mathf.PI / 2f, 2f * step) - step;
        float r = Mathf.Lerp(innerR, outerR, Mathf.Cos(a * n) * 0.5f + 0.5f);
        return dist - r;
    }

    private static void EnsureShieldSprite()
    {
        if (_shieldSprite != null) return;
        int s = 32;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float cx = (x + 0.5f) / s * 2f - 1f;
                float cy = (y + 0.5f) / s * 2f - 1f;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);
                float outer = 0.9f;
                float inner = 0.55f;
                float ring = Mathf.Abs(dist - (outer + inner) * 0.5f) - (outer - inner) * 0.5f;
                float alpha = Mathf.Clamp01(-ring * 12f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _shieldSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
    }
}

// ---- 拾取触发器（挂在子对象）----
public class PickupTrigger : MonoBehaviour
{
    public ItemDrop owner;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && owner != null)
            owner.OnPlayerPickup();
    }
}
