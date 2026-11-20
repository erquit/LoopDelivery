using UnityEngine;

public class TestClockSubscribe : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        GlobalClockManager.Instance.SubscribeToGameTime(new GameTime(7, 30, 0), OnFiveSecondsPassed);
    }

    private void OnFiveSecondsPassed()
    {
        Debug.Log("Five seconds have passed!");
    }
}
