using UnityEngine;
using UnityEngine.InputSystem;

public class TestPauseClock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            GlobalClockManager clockManager = GlobalClockManager.Instance;
            if (clockManager != null)
            {
                if (clockManager.IsClockRunning)
                {
                    clockManager.StopClock();
                    Debug.Log("Clock paused.");
                }
                else
                {
                    clockManager.StartClock();
                    Debug.Log("Clock resumed.");
                }
            }
        }
    }
}
