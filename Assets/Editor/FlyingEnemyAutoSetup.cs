using UnityEditor;
using UnityEngine;

public class FlyingEnemyAutoSetup : EditorWindow
{
    [MenuItem("Tools/Auto Setup Flying Enemy %#F")]
    static void SetupFlyingEnemy()
    {
        SetupMultipleFlyingEnemies(6);
    }

    static void SetupMultipleFlyingEnemies(int count)
    {
        // 加载小鸟精灵图
        Sprite birdSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/游戏资源/精灵图素材/角色/小怪/Bird.png");

        // 加载小鸟动画控制器（扇翅膀）
        RuntimeAnimatorController birdController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/游戏资源/动画/Bird.controller");

        // 找玩家位置作为起始参考
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 basePos = player != null ? player.transform.position : new Vector3(0, 0, 0);

        // 创建巡逻点根节点
        GameObject patrolRoot = GameObject.Find("__FlyingPatrolPoints__");
        if (patrolRoot == null)
            patrolRoot = new GameObject("__FlyingPatrolPoints__");

        // 先清理旧的飞行小怪和巡逻点
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("FlyingEnemy_"))
                DestroyImmediate(obj);
        }
        Transform[] children = patrolRoot.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != patrolRoot.transform)
                DestroyImmediate(child.gameObject);
        }

        // 沿关卡分布飞行小怪
        for (int i = 0; i < count; i++)
        {
            float xOffset = 8f + i * 7f;
            float yOffset = 2.0f + (i % 3) * 0.8f;

            // 创建飞行小怪
            GameObject flyObj = new GameObject("FlyingEnemy_" + i);
            flyObj.transform.position = new Vector3(basePos.x + xOffset, basePos.y + yOffset, 0);

            // 添加 FlyingEnemy 脚本
            FlyingEnemy flyScript = flyObj.AddComponent<FlyingEnemy>();

            // 添加 SpriteRenderer
            SpriteRenderer sr = flyObj.AddComponent<SpriteRenderer>();
            if (birdSprite != null)
            {
                sr.sprite = birdSprite;
            }
            else
            {
                GameObject existingEnemy = GameObject.Find("敌人1");
                if (existingEnemy != null)
                {
                    SpriteRenderer refSr = existingEnemy.GetComponentInChildren<SpriteRenderer>();
                    if (refSr != null && refSr.sprite != null)
                        sr.sprite = refSr.sprite;
                }
                Debug.LogWarning("[FlyingEnemySetup] 未找到 Bird.png，使用备用图片");
            }

            // 添加 Animator（扇翅膀动画）
            Animator animator = flyObj.AddComponent<Animator>();
            if (birdController != null)
            {
                animator.runtimeAnimatorController = birdController;
            }
            else
            {
                Debug.LogWarning("[FlyingEnemySetup] 未找到 Bird.controller，小鸟不会有扇翅膀动画");
            }

            // 添加 Rigidbody2D（重力=0）
            Rigidbody2D rb = flyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 添加 BoxCollider2D（Trigger）
            BoxCollider2D col = flyObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            // 设置 Layer 和 Tag
            flyObj.layer = LayerMask.NameToLayer("Enemy");
            flyObj.tag = "Enemy";

            // 缩放和排序
            flyObj.transform.localScale = new Vector3(2f, 2f, 2f);
            sr.sortingOrder = 1;

            // 创建巡逻点
            GameObject p1 = new GameObject("FlyPatrol_" + i + "_0");
            p1.transform.SetParent(patrolRoot.transform);
            p1.transform.position = new Vector3(basePos.x + xOffset - 2.5f, basePos.y + yOffset, 0);

            GameObject p2 = new GameObject("FlyPatrol_" + i + "_1");
            p2.transform.SetParent(patrolRoot.transform);
            p2.transform.position = new Vector3(basePos.x + xOffset + 2.5f, basePos.y + yOffset, 0);

            // 配置飞行小怪参数
            flyScript.patrolPoints = new Transform[] { p1.transform, p2.transform };
            flyScript.maxLife = 1;
            flyScript.damage = 1;
            flyScript.moveSpeed = 2.5f + (i % 3) * 0.5f;

            EditorUtility.SetDirty(flyObj);
            EditorUtility.SetDirty(flyScript);
        }

        Debug.Log("[FlyingEnemySetup] 已生成 " + count + " 个飞行小怪（含扇翅膀动画）！");
        Selection.activeGameObject = patrolRoot;
    }
}
