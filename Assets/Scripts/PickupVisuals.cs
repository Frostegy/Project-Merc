using UnityEngine;

public class PickupVisuals : MonoBehaviour
{
    [Header("Bob")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Rotate")]
    [SerializeField] private float rotateSpeed = 45f;

    [Header("Optional Glow Pulse")]
    [SerializeField] private Light glowLight;
    [SerializeField] private float minIntensity = 1.2f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float pulseSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = startPos + new Vector3(0f, bobOffset, 0f);

        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        if (glowLight != null)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            glowLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}
