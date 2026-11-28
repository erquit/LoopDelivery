using System;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-100)] // this script's Awake/OnEnable run before other scripts'
public class GlobalClockManager : MonoBehaviour
{
    public static GlobalClockManager Instance;
    
    // Time of Day. Use these to determine the current time of day in the game. For example, you can use this to change lighting, NPC behavior, etc.
    public enum TimeOfDay
    {
        None = 0,
        Morning,
        Midday,
        Afternoon,
        Dusk,
        Night
    }

    public event Action<TimeOfDay> OnTimeOfDayChanged; // Event for time of day changes
    private readonly Dictionary<GameTime, Action> subscribers = new Dictionary<GameTime, Action>(); // Dictionary to hold subscribers for specific GameTime events
    public event Action OnClockStarted; // Event for clock start
    public event Action OnClockStopped; // Event for clock stop
    
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

    private TimeOfDay lastTimeOfDay = TimeOfDay.None; // Track the last time of day to detect changes

    private float realTime = 0f; // Current time in seconds
    private bool isClockRunningState = false;

    private uint nextInGameSecondToProcess = 0; // Track the next second to process for event triggering

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

        StartClock();
        Debug.Log("Global Clock Manager initialized.");
        nextInGameSecondToProcess = 0; // Initialize the next second to process
    }

    void FixedUpdate()
    {
        if (isClockRunningState)
        {
            realTime += Time.fixedDeltaTime;

            /**
             check subscribers for the current game time and invoke their callbacks if the time has passed. 
             This ensures that we only invoke callbacks once per in-game second, even if FixedUpdate is 
             called multiple times within that second.
            */
            uint now = GetCurrentGameTimeInSeconds();
            while (nextInGameSecondToProcess <= now)
            {
                GameTime currentGameTime = new GameTime(nextInGameSecondToProcess);
                if (subscribers.ContainsKey(currentGameTime))
                {
                    subscribers[currentGameTime]?.Invoke();
                }
                nextInGameSecondToProcess++;
            }

            // Check for time of day changes and invoke the event if it has changed only at the start of a new time of day
            TimeOfDay currentTimeOfDay = GetCurrentTimeOfDay();
            if (lastTimeOfDay < currentTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
                lastTimeOfDay++;
            }

        }
    }
    
    public void StartClock()
    {
        isClockRunningState = true;
        OnClockStarted?.Invoke();
    }

    public void StopClock() => StopClock(true);

    public void StopClock(bool roundToMinute)
    {
        isClockRunningState = false;
        if (roundToMinute)
        {
            const float displayedUnit = 60f;
            float gameTime = realTime * ConversionRate;
            gameTime = Mathf.Floor(gameTime / displayedUnit) * displayedUnit;
            realTime = (gameTime + 0.001f) / ConversionRate;
        }
        OnClockStopped?.Invoke();
    }

    public bool TryAdvanceTo(uint gameTimeInSeconds)
    {
        if (gameTimeInSeconds <= GetCurrentGameTimeInSeconds() || gameTimeInSeconds > gameClockEndHour * 3600)
            return false;

        // Keep the float conversion inside the requested second when the clock truncates it.
        realTime = (gameTimeInSeconds - gameClockStartHour * 3600 + 0.01f) / ConversionRate;
        return true;
    }

    private float ConversionRate => (gameClockEndHour - gameClockStartHour) * 3600f / (realLoopTimeInMinutes * 60f);

    public void ResetClock()
    {
        realTime = 0f;
        nextInGameSecondToProcess = 0; // Reset the next second to process
        lastTimeOfDay = TimeOfDay.None; // Reset the last time of day
    }

    public bool IsClockRunning
    {
        get { return isClockRunningState; }
    }

    public uint GetCurrentGameTimeInSeconds()
    {
        uint gameTimeInSeconds = (uint)(ConversionRate * realTime);

        uint gameHours = gameClockStartHour + (gameTimeInSeconds / 3600) % 24;
        uint gameMinutes = gameTimeInSeconds % 3600 / 60;

        uint gameSeconds = gameTimeInSeconds % 60;

        // Debug.Log("Current Game Time: " + gameHours + ":" + gameMinutes + ":" + gameSeconds);

        return gameHours * 3600 + gameMinutes * 60 + gameSeconds; // Return total seconds since start of the game day
    }

    public TimeOfDay GetCurrentTimeOfDay()
    {
        uint now = GetCurrentGameTimeInSeconds();

        if (now >= gameNightStartTime.ToTotalSeconds())     return TimeOfDay.Night;
        if (now >= gameDuskStartTime.ToTotalSeconds())      return TimeOfDay.Dusk;
        if (now >= gameAfternoonStartTime.ToTotalSeconds()) return TimeOfDay.Afternoon;
        if (now >= gameMiddayStartTime.ToTotalSeconds())    return TimeOfDay.Midday;
        if (now >= gameMorningStart.ToTotalSeconds())       return TimeOfDay.Morning;
        return TimeOfDay.Night; // before morning starts, it's still night
    }

    // TODO: Make event handlers for time of day changes, so other scripts can subscribe to these events and react accordingly.

    public void SubscribeToGameTime(GameTime gameTime, Action callback)
    {
        if (!subscribers.ContainsKey(gameTime))
        {
            subscribers[gameTime] = callback;
        }
        else
        {
            subscribers[gameTime] += callback; // Add to existing subscribers for this GameTime
        }
    }

    public void UnsubscribeFromGameTime(GameTime gameTime, Action callback)
    {
        if (subscribers.ContainsKey(gameTime))
        {
            subscribers[gameTime] -= callback; // Remove the callback from the subscribers for this GameTime

            // If there are no more subscribers for this GameTime, remove the entry from the dictionary
            if (subscribers[gameTime] == null)
            {
                subscribers.Remove(gameTime);
            }
        }
    }
}
