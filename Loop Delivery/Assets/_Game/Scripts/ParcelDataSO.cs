using UnityEngine;

[CreateAssetMenu(fileName = "NewParcel", menuName = "Delivery/Parcel Data")]
public class ParcelDataSO : ScriptableObject
{
    public int parcelID;
    public string description;
    public BuildingDataSO destination;
}
