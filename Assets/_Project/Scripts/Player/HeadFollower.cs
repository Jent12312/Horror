using UnityEngine;

public class HeadFollower : MonoBehaviour
{
    [Tooltip(" ость головы вашей модели (куда должна крепитьс€ камера)")]
    public Transform targetHeadBone;

    [Tooltip("—двиг камеры вперед, чтобы она была на уровне глаз, а не в центре шеи")]
    public Vector3 offset = new Vector3(0, 0.1f, 0.15f);

    private void LateUpdate()
    {
        if (targetHeadBone == null) return;

        transform.position = targetHeadBone.TransformPoint(offset);
    }
}