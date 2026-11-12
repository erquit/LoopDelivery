using UnityEngine;

public class GlobalClockManager : MonoBehaviour
{
    public static GlobalClockManager Instance;

    [Header("Clock Settings")]
    public uint gameClockStartHour = 7; // Default start hour (7 AM)
    public uint gameClockEndHour = 24; // Default end hour (00 AM)
    public uint realLoopTimeInMinutes = 10; // Default loop time in minutes

    private float realTime = 0f; // Current time in seconds
    private bool isClockRunningState = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Destroy the duplicate
        }

        startClock();
        Debug.Log("Global Clock Manager initialized.");
    }

    void FixedUpdate()
    {
        if (isClockRunningState)
        {
            realTime += Time.fixedDeltaTime;

        }
    }
    
    public void startClock()
    {
        isClockRunningState = true;
    }

    public void stopClock()
    {
        isClockRunningState = false;

        // Make the time stop at the nearest displayed minute for better visual consistency
        float secondsInGameDay = 1f * (gameClockEndHour - gameClockStartHour) * 3600;

        float conversionRate = secondsInGameDay / (realLoopTimeInMinutes * 60f);

        float gameTime = realTime * conversionRate;

        const float displayedUnit = 60f; // One displayed minute
        gameTime = Mathf.Floor(gameTime / displayedUnit) * displayedUnit;

        const float boundaryEpsilon = 0.001f; // In-game seconds
        realTime = (gameTime + boundaryEpsilon) / conversionRate;
    }

    public bool isClockRunning
    {
        get { return isClockRunningState; }
    }

    public uint getCurrentGameTimeInSeconds()
    {
        uint secondsInGameDay = (gameClockEndHour - gameClockStartHour) * 3600; // Total seconds in the game day
        uint gameTimeInSeconds = (uint)(1f * secondsInGameDay / (realLoopTimeInMinutes * 60) * realTime); // Calculate game time in seconds based on real time passed

        uint gameHours = gameClockStartHour + (gameTimeInSeconds / 3600) % 24;
        uint gameMinutes = gameTimeInSeconds % 3600 / 60;

        uint gameSeconds = gameTimeInSeconds % 60;

        // Debug.Log("Current Game Time: " + gameHours + ":" + gameMinutes + ":" + gameSeconds);

        return gameHours * 3600 + gameMinutes * 60 + gameSeconds; // Return total seconds since start of the game day
    }
}
