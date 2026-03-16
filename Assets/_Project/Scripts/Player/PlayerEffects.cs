using UnityEngine;
using DG.Tweening; 

public class PlayerEffects : MonoBehaviour
{
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private Transform stoneVisualRoot; 
    [SerializeField] private Light stoneLight; 
    [SerializeField] private Light stoneLight2; 

    private void Awake()
    {
        if (stoneVisualRoot != null)
            stoneVisualRoot.localScale = Vector3.zero;
    }

    private void OnEnable() => equipment.OnStoneToggled += HandleStoneEffect;

    private void OnDisable() => equipment.OnStoneToggled -= HandleStoneEffect;

    private void HandleStoneEffect(bool isActive)
    {
        if (stoneVisualRoot == null) return;

        stoneVisualRoot.DOKill();

        if (isActive)
        {
            stoneVisualRoot.gameObject.SetActive(true);

            stoneVisualRoot.DOScale(Vector3.one * 10, 0.4f)
                .SetEase(Ease.OutBack);

            // 3. Можно добавить звук (если есть AudioManager)
            // AudioManager.PlaySound("StoneEquip", transform.position);
        }
        else
        {
            stoneVisualRoot.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    stoneVisualRoot.gameObject.SetActive(false);
                });
        }
    }
}