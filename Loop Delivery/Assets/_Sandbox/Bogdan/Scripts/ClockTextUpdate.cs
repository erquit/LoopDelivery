using TMPro;
using UnityEngine;

public class ClockTextUpdate : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI clockTextMesh;
    [SerializeField]
    private TextMeshProUGUI clockTimeOfDayTextMesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                clockTextMesh.color = Color.black;
            }
            else
            {
                clockTextMesh.color = Color.darkGray;
            }

            GlobalClockManager.TimeOfDay currentTimeOfDay = clockManager.getCurrentTimeOfDay();
            clockTimeOfDayTextMesh.text = currentTimeOfDay.ToString();
            clockTimeOfDayTextMesh.color = Color.black;

        } else {
            clockTextMesh.text = "Clock Manager not found";
        }
    }
}
