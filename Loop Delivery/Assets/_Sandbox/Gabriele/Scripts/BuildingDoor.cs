using UnityEngine;

public class BuildingDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingDataSO building;

    public void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PackageInventory inventory))
        {
            Debug.Log("BuildingDoor: The player does not have a PackageInventory.");
            return;
        }

        int delivered = inventory.DeliverTo(building);
        if (delivered > 0)
            Debug.Log("Thank you for the delivery!");
        else
            Debug.Log("Wrong address!");
    }
}
