using UnityEngine;
using RootMotion.FinalIK;
using Unity.Netcode;
using DG.Tweening;

public class PlayerIKController : NetworkBehaviour
{
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private BipedIK bipedIK;
    [SerializeField] private Transform handTarget; 

    private void OnEnable() => equipment.OnStoneToggled += UpdateHandIK;

    private void OnDisable() => equipment.OnStoneToggled -= UpdateHandIK;

    private void UpdateHandIK(bool isStoneActive)
    {
        if (bipedIK == null) return;

        if (isStoneActive)
        {
            bipedIK.solvers.rightHand.target = handTarget;

            DOTween.To(() => bipedIK.solvers.rightHand.IKPositionWeight,
                       x => bipedIK.solvers.rightHand.IKPositionWeight = x,
                       1f, 0.3f);

            bipedIK.solvers.rightHand.IKRotationWeight = 1f;
        }
        else
        {
            DOTween.To(() => bipedIK.solvers.rightHand.IKPositionWeight,
                       x => bipedIK.solvers.rightHand.IKPositionWeight = x,
                       0f, 0.3f);
            bipedIK.solvers.rightHand.IKRotationWeight = 0f;
        }
    }

    public void UpdateIKForHeldItem(InteractableBase item)
    {
        if (item == null)
        {
            // Сбрасываем все веса в 0, если ничего не несем
            bipedIK.solvers.rightHand.IKPositionWeight = 0;
            bipedIK.solvers.leftHand.IKPositionWeight = 0;
            return;
        }

        // 1. Правая рука всегда тянется к предмету
        bipedIK.solvers.rightHand.target = item.rightHandGrip;
        bipedIK.solvers.rightHand.IKPositionWeight = 1f;
        bipedIK.solvers.rightHand.IKRotationWeight = 1f;

        // 2. Левая рука - только если предмет большой
        if (item.holdType == ItemHoldType.LargeTwoHanded)
        {
            bipedIK.solvers.leftHand.target = item.leftHandGrip;
            bipedIK.solvers.leftHand.IKPositionWeight = 1f;
            bipedIK.solvers.leftHand.IKRotationWeight = 1f;
        }
        else
        {
            bipedIK.solvers.leftHand.IKPositionWeight = 0f; // Левая рука свободна (анимация бега)
        }
    }
}