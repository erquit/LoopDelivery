using UnityEngine;
using System.Collections.Generic;

public class PackageInventory : MonoBehaviour
{
    private readonly List<PackageDataSO> packages = new();
    public IReadOnlyList<PackageDataSO> Packages => packages;

    public void Add(PackageDataSO package)
    {
        if (package == null) return;
        packages.Add(package);
    }

    public int DeliverTo(BuildingDataSO building)
    {
        return packages.RemoveAll(p => p.destination == building);
    }
}
