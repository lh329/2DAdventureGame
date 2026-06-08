using UnityEngine;

// Attach this ONLY to the MagicCarpet child object under Boss.
// Provides a flowing, wave-like floating motion.
public class CarpetFloat : MonoBehaviour
{
    [Header("Wave Motion — increase these if too subtle")]
    public float floatAmp = 0.06f;
    public float floatSpeed = 2.5f;

    [Header("Tilt / Roll (front-to-back wave rocking)")]
    public float tiltAmp = 12f;
    public float tiltSpeed = 2.0f;

    [Header("Horizontal drift (snake-like sway)")]
    public float swayAmp = 0.025f;
    public float swaySpeed = 2.5f;

    private Vector3 baseLocalPos;
    private float timeOffset;

    void Start()
    {
        baseLocalPos = transform.localPosition;
        timeOffset = Random.Range(0f, 3f);
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        // 1. Vertical float — compound sine for organic, non-repeating wave
        float y = Mathf.Sin(t * floatSpeed) * floatAmp
                + Mathf.Sin(t * floatSpeed * 1.7f + 1.1f) * (floatAmp * 0.5f);

        // 2. Horizontal sway
        float x = Mathf.Sin(t * swaySpeed + 0.8f) * swayAmp;

        // Apply position (carpet only, Boss untouched)
        transform.localPosition = baseLocalPos + new Vector3(x, y, 0f);

        // 3. Tilt — pronounced front-to-back rocking like a seesaw / wave
        float zRot = Mathf.Sin(t * tiltSpeed) * tiltAmp
                   + Mathf.Sin(t * tiltSpeed * 2.5f + 0.7f) * (tiltAmp * 0.4f);
        transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
    }
}
