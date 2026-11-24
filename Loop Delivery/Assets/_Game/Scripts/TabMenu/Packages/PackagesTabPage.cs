using UnityEngine;

public class PackagesTabPage : MonoBehaviour
{   
    [SerializeField] private Transform packagesTabContent;
    [SerializeField] private PackageListItem packagesTabItemPrefab;

    private PlayerController playerController;
    private ParcelInventory playerInventory;

    void OnEnable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player GameObject not found in the scene.");
            return;
        }

        playerController = player.GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        playerInventory = player.GetComponent<ParcelInventory>();
        if (playerInventory == null)
        {
            Debug.LogWarning("ParcelInventory component not found on the player.");
            return;
        }
        playerInventory.enabled = false;

        RefreshList();
    }

    void OnDisable()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerInventory != null) playerInventory.enabled = true;
    }

    private void RefreshList()
    {
        // Clear the rows from the last time the tab was opened
        foreach (Transform child in packagesTabContent)
        {
            Destroy(child.gameObject);
        }

        foreach (ParcelDataSO parcel in playerInventory.Parcels)
        {
            PackageListItem item = Instantiate(packagesTabItemPrefab, packagesTabContent);
            item.Setup(parcel);
        }
    }
}
