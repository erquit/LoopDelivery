using UnityEngine;

public class BossNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private ParcelDataSO[] parcelsToGive;
    private bool alreadyGave = false;

    public void Interact(GameObject interactor)
    {
        interactor.TryGetComponent(out ParcelInventory inventory);
        if (alreadyGave)
        {
            if (inventory.AllDelivered)
                Debug.Log("Boss: Congratulations! Your shift is over, no other deliveries for today.");
            else            
                Debug.Log("Boss: I already gave you the parcels, go away!");
            return;
        }

        foreach (ParcelDataSO parcel in parcelsToGive)
        {
            inventory.Add(parcel);
        }
        alreadyGave = true;
        Debug.Log($"Boss: Here are {parcelsToGive.Length} parcels to deliver! Good luck!");
    }
}
