using TMPro;
using UnityEngine;

public class PackageListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI destinationText;
    
    public void Setup(ParcelDataSO parcel)
    {
        idText.text = $"#{parcel.parcelID}";
        destinationText.text = $"Destination: {parcel.destination.displayName}";
    }
}
