using UnityEngine;
using System.Collections.Generic;

public class ParcelInventory : MonoBehaviour
{
    private readonly List<ParcelDataSO> parcels = new();
    
    public IReadOnlyList<ParcelDataSO> Parcels => parcels;
    public bool AllDelivered => parcels.Count == 0;

    public void Add(ParcelDataSO parcel)
    {
        if (parcel == null) return;
        parcels.Add(parcel);
    }

    public int DeliverTo(BuildingDataSO building)
    {
        return parcels.RemoveAll(p => p.destination == building);
    }
}
