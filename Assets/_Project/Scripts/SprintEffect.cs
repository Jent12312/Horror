using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class SprintEffect : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private Camera playerCamera;

    [BoxGroup("FOV")]
    [SerializeField] private float normalFOV = 75f;
    [BoxGroup("FOV")]
    [SerializeField] private float sprintFOV = 9f;
    [BoxGroup("FOV")]
    [SerializeField] private float fovDuration = 0.3f;
    [BoxGroup("FOV")]
    [SerializeField] private Ease fovEase = Ease.OutQuart;

    [BoxGroup("Tilt")]
    [SerializeField] private float tiltAngle = 1.5f;
    [BoxGroup("Tilt")]
    [SerializeField] private float tiltDuration = 0.25f;
    [BoxGroup("Tilt")]
    [SerializeField] private Ease tiltEase = Ease.OutSine;

    private Tweener fovTween;
    private Tweener tiltTween;
    private bool isActive;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    public void SetSprinting(bool sprinting)
    {
        if (sprinting == isActive) return;
        isActive = sprinting;

        AnimateFOV(sprinting ? sprintFOV : normalFOV);
        AnimateTilt(sprinting ? tiltAngle : 0f);
    }

    private void AnimateFOV(float target)
    {
        fovTween?.Kill();
        fovTween = playerCamera
            .DOFieldOfView(target, fovDuration)
            .SetEase(fovEase)
            .SetUpdate(true); 
    }

    private void AnimateTilt(float target)
    {
        tiltTween?.Kill();
        tiltTween = playerCamera.transform
            .DOLocalRotate(new Vector3(
                playerCamera.transform.localEulerAngles.x,
                playerCamera.transform.localEulerAngles.y,
                target), tiltDuration)
            .SetEase(tiltEase);
    }

    private void OnDestroy()
    {
        fovTween?.Kill();
        tiltTween?.Kill();
    }
}