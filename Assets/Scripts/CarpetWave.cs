using UnityEngine;

/// <summary>
/// Soft magic carpet — generates a mesh deformed by traveling sine waves.
/// Attach ONLY to MagicCarpet child object under Boss.
/// Replaces SpriteRenderer with a custom mesh.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CarpetWave : MonoBehaviour
{
    [Header("Mesh Size")]
    public int segments = 20;          // More = smoother wave
    public float width = 2.8f;         // World width of carpet
    public float height = 0.45f;       // World height of carpet body

    [Header("Wave Parameters")]
    public float waveSpeed = 0.8f;     // Slow traveling wave
    public float waveAmp = 0.01f;      // Barely noticeable ripple
    public float waveFreq = 2f;        // Gentle waves along length

    [Header("Vertical Float")]
    public float floatAmp = 0.004f;    // Almost imperceptible bob
    public float floatSpeed = 0.9f;

    [Header("Soft Tilt")]
    public float tiltAmp = 0.6f;
    public float tiltSpeed = 1.0f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3 baseLocalPos;
    private float timeOffset;

    void Start()
    {
        baseLocalPos = transform.localPosition;
        timeOffset = Random.Range(0f, 3f);
        BuildMesh();
    }

    void BuildMesh()
    {
        mesh = new Mesh();
        mesh.name = "MagicCarpetMesh";

        // Create vertices for a flat quad divided into segments
        int vertCount = (segments + 1) * 2; // top row + bottom row
        int triCount = segments * 6;         // 2 triangles per segment

        baseVertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] tris = new int[triCount];

        float halfW = width / 2f;
        float halfH = height / 2f;

        int vi = 0;
        for (int i = 0; i <= segments; i++)
        {
            float x = -halfW + (i / (float)segments) * width;
            // Top edge
            baseVertices[vi] = new Vector3(x, halfH, 0f);
            uvs[vi] = new Vector2(i / (float)segments, 1f);
            vi++;
            // Bottom edge
            baseVertices[vi] = new Vector3(x, -halfH, 0f);
            uvs[vi] = new Vector2(i / (float)segments, 0f);
            vi++;
        }

        // Triangles
        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int tl = i * 2;
            int tr = tl + 1;
            int bl = tl + 2;
            int br = tl + 3;
            // Triangle 1
            tris[ti++] = tl;
            tris[ti++] = bl;
            tris[ti++] = tr;
            // Triangle 2
            tris[ti++] = tr;
            tris[ti++] = bl;
            tris[ti++] = br;
        }

        mesh.vertices = baseVertices;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if (mesh == null || baseVertices == null) return;

        float t = Time.time + timeOffset;
        Vector3[] verts = new Vector3[baseVertices.Length];
        System.Array.Copy(baseVertices, verts, baseVertices.Length);

        float halfW = width / 2f;

        // Apply wave deformation to each vertex column
        for (int i = 0; i <= segments; i++)
        {
            float xPos = -halfW + (i / (float)segments) * width;
            // Traveling wave phase — moves left to right over time
            float phase = (xPos / width) * waveFreq * Mathf.PI * 2f - t * waveSpeed;
            // Vertical displacement — stronger at bottom edge (fringe area)
            float waveY = Mathf.Sin(phase) * waveAmp;

            // Bottom vertices get more displacement (fringe hangs down)
            int ti = i * 2;      // top index
            int bi = i * 2 + 1;   // bottom index

            // Both edges get some wave, but bottom gets more
            verts[ti].y += waveY * 0.6f;
            verts[bi].y += waveY * 1.2f;

            // Slight X displacement for organic feel (bottom more)
            float waveX = Mathf.Cos(phase) * waveAmp * 0.15f;
            verts[bi].x += waveX;
        }

        mesh.vertices = verts;
        mesh.RecalculateNormals();

        // Gentle overall vertical bob
        float bobY = Mathf.Sin(t * floatSpeed) * floatAmp;
        // Soft tilt (like the whole carpet gently rocks)
        float zRot = Mathf.Sin(t * tiltSpeed + 1f) * tiltAmp;
        transform.localRotation = Quaternion.Euler(0f, 0f, zRot);

        transform.localPosition = baseLocalPos + new Vector3(0f, bobY, 0f);
    }

    void OnDestroy()
    {
        if (mesh != null)
        {
            Destroy(mesh);
        }
    }
}
