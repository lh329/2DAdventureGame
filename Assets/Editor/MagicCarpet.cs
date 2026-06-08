using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MagicCarpet : EditorWindow
{
    [MenuItem("Tools/Remove All Carpets")]
    static void RemoveAllCarpets()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot modify scene while in Play Mode! Exit play mode first.");
            return;
        }
        GameObject boss = GameObject.Find("Boss");
        if (boss == null)
        {
            Debug.LogError("Boss not found in scene!");
            return;
        }

        int removed = 0;
        for (int i = boss.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = boss.transform.GetChild(i);
            if (child.name.Contains("Carpet") || child.name.Contains("carpet"))
            {
                DestroyImmediate(child.gameObject);
                removed++;
            }
        }
        Debug.Log($"Removed {removed} carpet objects from Boss.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Add Magic Carpet")]
    static void AddMagicCarpet()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot modify scene while in Play Mode! Exit play mode first.");
            return;
        }
        GameObject boss = GameObject.Find("Boss");
        if (boss == null)
        {
            Debug.LogError("Boss not found in scene!");
            return;
        }

        // Remove existing carpets first
        for (int i = boss.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = boss.transform.GetChild(i);
            if (child.name.Contains("Carpet") || child.name.Contains("carpet"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Load the MagicCarpet texture by GUID
        string path = AssetDatabase.GUIDToAssetPath("7f44d62734174af2bf578b8c68a45926");
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            // Fallback: try to find by filename
            string[] guids = AssetDatabase.FindAssets("MagicCarpet t:Texture");
            if (guids.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[0]);
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
        if (tex == null)
        {
            Debug.LogError("MagicCarpet texture not found! Make sure MagicCarpet.png is in Assets/Sprites/");
            return;
        }

        // Create carpet as Boss child
        GameObject carpet = new GameObject("MagicCarpet");
        carpet.transform.SetParent(boss.transform, false);

        // Position: Boss stands ON the carpet — no overlap!
        // Boss center to feet ~0.33 local units.
        // Carpet height 0.08 (world 0.32), half = 0.04 local.
        // To place top edge right at y=-0.33: center = -0.33 - 0.04 = -0.37
        carpet.transform.localPosition = new Vector3(0f, -0.37f, 0f);
        carpet.transform.localRotation = Quaternion.identity;
        carpet.transform.localScale = Vector3.one;

        // Add MeshRenderer + MeshFilter (CarpetWave will create the mesh)
        carpet.AddComponent<MeshFilter>();
        MeshRenderer mr = carpet.AddComponent<MeshRenderer>();
        
        // Create material with carpet texture
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = tex;
        mat.SetFloat("_EnableExternalAlpha", 0);
        mr.material = mat;
        mr.sortingLayerName = "Enemy";
        mr.sortingOrder = -1; // Render behind Boss

        // Add wave deformation script
        CarpetWave wave = carpet.AddComponent<CarpetWave>();
        // Very subtle wave — almost flat, just barely alive
        wave.width = 0.9f;
        wave.height = 0.08f;
        wave.segments = 20;
        wave.waveSpeed = 0.8f;
        wave.waveAmp = 0.01f;
        wave.waveFreq = 2f;
        wave.floatAmp = 0.004f;
        wave.floatSpeed = 0.9f;
        wave.tiltAmp = 0.6f;
        wave.tiltSpeed = 1.0f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Soft wave carpet added under Boss!");
        Selection.activeGameObject = carpet;
    }
}
