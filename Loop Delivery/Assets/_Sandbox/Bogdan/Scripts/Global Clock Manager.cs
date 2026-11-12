using UnityEngine;


public class GlobalClockManager : MonoBehaviour
{
    public static GlobalClockManager Instance;

    [Header("Clock Settings")]
    public uint gameClockStartHour = 7; // Default start hour (7 AM)
    public uint gameClockEndHour = 24; // Default end hour (00 AM)
    public uint realLoopTimeInMinutes = 10; // Default loop time in minutes

    [Header("Time of Day Settings")]
    private GameTime gameMorningStart; // is initialized in Awake() to gameClockStartHour
    public GameTime gameMiddayStartTime = new GameTime(11, 0, 0);
    public GameTime gameAfternoonStartTime = new GameTime(13, 0, 0);
    public GameTime gameDuskStartTime = new GameTime(18, 30, 0);
    public GameTime gameNightStartTime = new GameTime(20, 0, 0);
    public enum TimeOfDay
    {
        Morning,
        Midday,
        Afternoon,
        Dusk,
        Night
    }

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

        gameMorningStart = new GameTime(gameClockStartHour, 0, 0); // Initialize morning start time based on gameClockStartHour

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

    public TimeOfDay getCurrentTimeOfDay()
    {
        uint currentGameTimeInSeconds = getCurrentGameTimeInSeconds();

        if (currentGameTimeInSeconds >= gameMorningStart.ToTotalSeconds() && currentGameTimeInSeconds < gameMiddayStartTime.ToTotalSeconds())
        {
            return TimeOfDay.Morning;
        }
        else if (currentGameTimeInSeconds >= gameMiddayStartTime.ToTotalSeconds() && currentGameTimeInSeconds < gameAfternoonStartTime.ToTotalSeconds())
        {
            return TimeOfDay.Midday;
        }
        else if (currentGameTimeInSeconds >= gameAfternoonStartTime.ToTotalSeconds() && currentGameTimeInSeconds < gameDuskStartTime.ToTotalSeconds())
        {
            return TimeOfDay.Afternoon;
        }
        else if (currentGameTimeInSeconds >= gameDuskStartTime.ToTotalSeconds() && currentGameTimeInSeconds < gameNightStartTime.ToTotalSeconds())
        {
            return TimeOfDay.Dusk;
        }
        else
        {
            return TimeOfDay.Night;
        }
    }

    // TODO: Make event handlers for time of day changes, so other scripts can subscribe to these events and react accordingly.
}
