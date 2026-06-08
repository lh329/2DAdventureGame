using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人死亡粒子碎片效果
/// 调用 DeathParticles.Spawn(position, color) 即可
/// </summary>
public static class DeathParticles
{
    public static void Spawn(Vector3 pos, Color tint)
    {
        // 用协程启动器运行异步逻辑
        var runner = new GameObject("DeathParticleRunner");
        var mono = runner.AddComponent<DeathParticleRunner>();
        mono.Run(pos, tint);
    }
}

public class DeathParticleRunner : MonoBehaviour
{
    public void Run(Vector3 pos, Color tint)
    {
        StartCoroutine(SpawnParticles(pos, tint));
    }

    private IEnumerator SpawnParticles(Vector3 pos, Color tint)
    {
        int count = 8;
        float lifeTime = 0.6f;
        float speed = 3f;
        float size = 0.15f;

        // 创建1x1白色纹理作为粒子贴图
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        GameObject[] particles = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("DeathParticle_" + i);
            obj.transform.position = pos + (Vector3)(Random.insideUnitCircle * 0.2f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            sr.sortingOrder = 50;
            sr.color = tint;
            obj.transform.localScale = Vector3.one * size * Random.Range(0.7f, 1.3f);

            particles[i] = obj;
        }

        // 粒子飞散+缩小+渐隐
        Vector2[] velocities = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f / count) * i + Random.Range(-0.3f, 0.3f);
            velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * Random.Range(0.6f, 1.2f);
        }

        float t = 0f;
        while (t < lifeTime)
        {
            t += Time.deltaTime;
            float ratio = t / lifeTime;

            for (int i = 0; i < count; i++)
            {
                if (particles[i] == null) continue;

                // 飞散（逐渐减速）+ 重力
                velocities[i] *= 0.96f;
                velocities[i] += Vector2.down * 8f * Time.deltaTime;
                particles[i].transform.position += (Vector3)velocities[i] * Time.deltaTime;

                // 缩小+渐隐
                float scale = (1f - ratio) * 0.15f * Random.Range(0.8f, 1.2f);
                particles[i].transform.localScale = Vector3.one * scale;

                var sr = particles[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f - ratio;
                    sr.color = c;
                }
            }

            yield return null;
        }

        // 清理
        for (int i = 0; i < count; i++)
        {
            if (particles[i] != null)
                Destroy(particles[i]);
        }

        Destroy(tex);
        Destroy(gameObject);
    }
}
