using Unity.Netcode;
using UnityEngine;

public class ScoutAbilities : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Light flashlight; // Уникальный SpotLight Скаута
    [SerializeField] private GameObject candlePrefab;
    [SerializeField] private GameObject tripwirePrefab;
    [SerializeField] private Transform placePoint;

    private int candlesRemaining = 3;
    private int tripwiresRemaining = 2;
    private bool isFlashlightOn = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.ToggleLightEvent += ToggleFlashlight; // F (уже забиндено в InputReader)
        inputReader.Ability1Event += TryPlaceCandle;      // Q
        inputReader.Ability2Event += TryPlaceTripwire;    // E
        // Открытие логова добавим в Этапе 5
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.ToggleLightEvent -= ToggleFlashlight;
        inputReader.Ability1Event -= TryPlaceCandle;
        inputReader.Ability2Event -= TryPlaceTripwire;
    }

    private void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        ToggleFlashlightServerRpc(isFlashlightOn);
    }

    [ServerRpc]
    private void ToggleFlashlightServerRpc(bool state)
    {
        ToggleFlashlightClientRpc(state);
    }

    [ClientRpc]
    private void ToggleFlashlightClientRpc(bool state)
    {
        if (flashlight != null) flashlight.enabled = state;
    }

    private void TryPlaceCandle()
    {
        if (candlesRemaining > 0) PlaceItemServerRpc(0, placePoint.position);
    }

    private void TryPlaceTripwire()
    {
        if (tripwiresRemaining > 0) PlaceItemServerRpc(1, placePoint.position, placePoint.rotation);
    }

    [ServerRpc]
    private void PlaceItemServerRpc(int itemType, Vector3 pos, Quaternion rot = default)
    {
        if (itemType == 0 && candlesRemaining > 0) // Candle
        {
            candlesRemaining--;
            GameObject candle = Instantiate(candlePrefab, pos, Quaternion.identity);
            candle.GetComponent<NetworkObject>().Spawn();
        }
        else if (itemType == 1 && tripwiresRemaining > 0) // Tripwire
        {
            tripwiresRemaining--;
            GameObject tripwire = Instantiate(tripwirePrefab, pos, rot);
            tripwire.GetComponent<NetworkObject>().Spawn();
        }
    }

    // Вызывается растяжкой (Tripwire)
    [ClientRpc]
    public void RevealDevilPositionClientRpc(Vector3 devilPosition, float duration)
    {
        if (!IsOwner) return;
        Debug.Log($"[Scout] DEVIL SPOTTED at {devilPosition}!");
        // TODO: В будущем спавн временной UI-иконки или маркера на экране/мини-карте
    }
}