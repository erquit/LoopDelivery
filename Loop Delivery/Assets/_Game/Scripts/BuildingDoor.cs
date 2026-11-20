using UnityEngine;

// Script to be assigned to a child object of each building, in the position of the door to make it interactable
public class BuildingDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingDataSO building;

    public void Interact(GameObject interactor)
    {
        interactor.TryGetComponent(out ParcelInventory inventory);

        int delivered = inventory.DeliverTo(building);
        if (delivered > 0)
            Debug.Log("Thank you for the delivery!");
        else
            Debug.Log("Wrong address!");
    }
}
