using UnityEngine;

public class ItemAntiClipping : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float pushForwardAmount = 0.4f; // Насколько отодвигать
    [SerializeField] private float downAngleThreshold = 30f; // Угол, после которого начинается сдвиг

    private Vector3 initialLocalPos;

    void Start() => initialLocalPos = transform.localPosition;

    void Update()
    {
        // Получаем наклон камеры (0 - прямо, 90 - в пол)
        float angle = cameraTransform.localEulerAngles.x;
        if (angle > 180) angle -= 360; // Корректировка углов Unity

        float offsetZ = 0;

        if (angle > downAngleThreshold)
        {
            // Рассчитываем коэффициент сдвига (чем ниже смотрим, тем больше сдвиг)
            float normalizeAngle = Mathf.InverseLerp(downAngleThreshold, 90, angle);
            offsetZ = normalizeAngle * pushForwardAmount;
        }

        // Плавно меняем позицию HoldPoint
        Vector3 targetPos = initialLocalPos + new Vector3(0, 0, offsetZ);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 10f);
    }
}