using TMPro;
using UnityEngine;

public class ClockTextUpdate : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI clockTextMesh;
    [SerializeField]
    private TextMeshProUGUI clockTimeOfDayTextMesh;

    void Update()
    {
        HandleClockTimeChanged(GlobalClockManager.Instance.GetCurrentGameTimeInSeconds());
    }

    private void OnEnable()
    {
        GlobalClockManager.Instance.OnTimeOfDayChanged += HandleTimeOfDayChanged;
        GlobalClockManager.Instance.OnClockStarted += HandleClockStarted;
        GlobalClockManager.Instance.OnClockStopped += HandleClockStopped;
    }

    private void OnDisable()
    {
        GlobalClockManager.Instance.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        GlobalClockManager.Instance.OnClockStarted -= HandleClockStarted;
        GlobalClockManager.Instance.OnClockStopped -= HandleClockStopped;
    }

    private void HandleTimeOfDayChanged(GlobalClockManager.TimeOfDay timeOfDay)
    {
        clockTimeOfDayTextMesh.text = timeOfDay.ToString();
        clockTimeOfDayTextMesh.color = Color.black;
    }

    private void HandleClockStopped()
    {
        clockTextMesh.color = Color.gray;
        clockTimeOfDayTextMesh.color = Color.gray;
    }

    private void HandleClockStarted()
    {
        clockTextMesh.color = Color.black;
        clockTimeOfDayTextMesh.color = Color.black;
    }

    private void HandleClockTimeChanged(uint totalSeconds)
    {
        uint hours = totalSeconds / 3600;
        uint minutes = (totalSeconds % 3600) / 60;

        string formattedTime = string.Format("{0:D2}:{1:D2}", hours, minutes);
        clockTextMesh.text = formattedTime;
    }
}
