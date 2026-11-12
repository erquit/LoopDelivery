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
                if (clockManager.isClockRunning)
                {
                    clockManager.stopClock();
                    Debug.Log("Clock paused.");
                }
                else
                {
                    clockManager.startClock();
                    Debug.Log("Clock resumed.");
                }
            }
        }
    }
}
