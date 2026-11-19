using UnityEngine;

[CreateAssetMenu(fileName = "NewPackage", menuName = "Delivery/Package Data")]
public class PackageDataSO : ScriptableObject
{
    public int packageID;
    public string description;
    public BuildingDataSO destination;
}
