using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SinusoidalStone : NetworkBehaviour
{
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float cycleDuration = 15.0f;
    [SerializeField] private float baseRange = 6.4f;
    [SerializeField] private Light light;
    [SerializeField] private Light light2;
    [SerializeField] private Renderer StoneRenderer;

    private float timeOffset;
    private Color emissionColor = Color.white;

    private void Awake()
    {
        timeOffset = Random.Range(0f, 100f);
        StoneRenderer.material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {

        float t = (Mathf.Sin((Time.time + timeOffset) * Mathf.PI * 2f / cycleDuration) + 1f) / 2f;

        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        light.range = baseRange * (0.8f + (0.2f * t));
        light2.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        light2.range = baseRange * (0.8f + (0.2f * t));
        Color finalColor = emissionColor * Mathf.Lerp(1, 4, t);
        StoneRenderer.material.SetColor("_EmissionColor", finalColor);
    }
}