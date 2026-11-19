using UnityEngine;

public class BossNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private PackageDataSO[] packagesToGive;
    private bool alreadyGave = false;

    public void Interact(GameObject interactor)
    {
        if (alreadyGave)
        {
            Debug.Log("Boss: I already gave you the packages, go away!");
            return;
        }

        if (!interactor.TryGetComponent(out PackageInventory inventory))
        {
            Debug.Log("BossNPC: The player does not have a PackageInventory component.");
            return;
        }

        foreach (PackageDataSO package in packagesToGive)
        {
            inventory.Add(package);
        }
        alreadyGave = true;
        Debug.Log($"Boss: Here are {packagesToGive.Length} packages to deliver! Good luck!");
    }
}
