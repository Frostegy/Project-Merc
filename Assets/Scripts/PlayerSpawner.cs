using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform spawnPoint;

    [Header("Scene References")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private GameObject crossHair;

    [Header("UI References")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private WeaponHud weaponHud;

    private GameObject spawnedPlayer;

    void Start()
    {
        SpawnSelectedPlayer();
    }

    void SpawnSelectedPlayer()
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogWarning("PlayerSpawner: No player prefabs assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("PlayerSpawner: No spawn point assigned.");
            return;
        }

        int index = Mathf.Clamp(GameSelection.selectedCharacterIndex, 0, playerPrefabs.Length - 1);
        GameObject prefab = playerPrefabs[index];

        if (prefab == null)
        {
            Debug.LogWarning("PlayerSpawner: Selected prefab is missing.");
            return;
        }

        spawnedPlayer = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        WirePlayerReferences(spawnedPlayer);
    }

    void WirePlayerReferences(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        PlayerInventory playerInventory = playerObject.GetComponent<PlayerInventory>();
        ActiveWeapon activeWeapon = playerObject.GetComponentInChildren<ActiveWeapon>();

        if (playerController != null)
        {
            if (gameplayCamera != null)
                playerController.gameplayCamera = gameplayCamera;

            if (normalCamera != null)
                playerController.normalCamera = normalCamera;

            if (aimCamera != null)
                playerController.aimCamera = aimCamera;

            if (crossHair != null)
                playerController.crossHair = crossHair;

            if (normalCamera != null && playerController.cinemachineCameraTarget != null)
            {
                normalCamera.Target.TrackingTarget = playerController.cinemachineCameraTarget;
            }

            if (aimCamera != null && playerController.cinemachineCameraTarget != null)
            {
                aimCamera.Target.TrackingTarget = playerController.cinemachineCameraTarget;
            }

            if (aimCamera != null && playerController.aimTarget != null)
            {
                aimCamera.Target.LookAtTarget = playerController.aimTarget;
            }
        }

        if (playerHealth != null)
        {
            if (healthSlider != null)
                playerHealth.healthSlider = healthSlider;

            if (healthText != null)
                playerHealth.healthText = healthText;
        }

        if (activeWeapon != null && playerController != null && playerController.aimTarget != null)
        {
            activeWeapon.crossHairTarget = playerController.aimTarget;
        }

        if (inventoryUI != null)
        {
            if (playerInventory != null)
                inventoryUI.inventory = playerInventory.gridInventory;

            if (activeWeapon != null)
                inventoryUI.activeWeapon = activeWeapon;

            if (playerHealth != null)
                inventoryUI.playerHealth = playerHealth;
        }

        if (weaponHud != null && activeWeapon != null)
        {
            weaponHud.activeWeapon = activeWeapon;
        }
    }
}