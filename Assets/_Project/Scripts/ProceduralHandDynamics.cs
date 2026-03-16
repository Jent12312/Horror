using UnityEngine;
using Unity.Netcode;

public class ProceduralHandDynamics : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Ссылка на Rigidbody корневого объекта Player")]
    [SerializeField] private Rigidbody playerRb;

    [Header("Jump & Fall Inertia")]
    [Tooltip("Сила смещения камня по Y при падении/прыжке")]
    [SerializeField] private float verticalSwayMultiplier = -0.05f;
    [Tooltip("Насколько сильно отталкивать камень ВПЕРЕД при прыжке, чтобы не лез в тело")]
    [SerializeField] private float pushForwardOnJump = 0.1f;
    [SerializeField] private float maxSway = 0.3f;
    [SerializeField] private float smoothness = 10f;

    private Vector3 initialLocalPosition;
    private Vector3 targetLocalPosition;

    public override void OnNetworkSpawn()
    {
        // Отключаем скрипт для чужих игроков, чтобы не тратить ресурсы
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Запоминаем идеальную позицию, которую вы настроили в Инспекторе
        initialLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (playerRb == null) return;

        // 1. Получаем вертикальную скорость персонажа (прыжок > 0, падение < 0)
        float velocityY = playerRb.linearVelocity.y;

        // 2. Рассчитываем смещение вверх/вниз (инерция)
        float offsetY = Mathf.Clamp(velocityY * verticalSwayMultiplier, -maxSway, maxSway);

        // 3. Рассчитываем отталкивание ВПЕРЕД (защита от проваливания в тело)
        // Берем модуль скорости (Abs), чтобы камень отталкивался вперед и при прыжке, и при падении
        float offsetZ = Mathf.Clamp(Mathf.Abs(velocityY) * (pushForwardOnJump * 0.1f), 0, maxSway);

        // 4. Формируем целевую позицию
        Vector3 swayOffset = new Vector3(0, offsetY, offsetZ);
        targetLocalPosition = initialLocalPosition + swayOffset;

        // 5. Плавно двигаем таргет (Lerp)
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * smoothness);
    }
}