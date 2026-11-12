using TMPro;
using UnityEngine;

public class ClockTextUpdate : MonoBehaviour
{
    private TextMeshProUGUI clockTextMesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clockTextMesh = GetComponent<TextMeshProUGUI>();
        if (clockTextMesh == null)
        {
            Debug.LogError("TextMeshPro component not found on the GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        GlobalClockManager clockManager = GlobalClockManager.Instance;
        if (clockManager != null)
        {
            uint currentGameTime = clockManager.getCurrentGameTimeInSeconds();
            uint hours = currentGameTime / 3600;
            uint minutes = (currentGameTime % 3600) / 60;

            string formattedTime = string.Format("{0:D2}:{1:D2}", hours, minutes);
            clockTextMesh.text = formattedTime;

            if (clockManager.isClockRunning)
            {
                clockTextMesh.color = Color.black; // Clock is running
            }
            else
            {
                clockTextMesh.color = Color.darkGray; // Clock is paused
            }

        } else {
            clockTextMesh.text = "Clock Manager not found";
        }
    }
}
