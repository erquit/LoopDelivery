using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-101)] // this script's Awake/OnEnable run before other scripts'
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool isGamePaused = false; // Track the game's pause state

    private readonly HashSet<NotebookEntryDataSO> discovered = new HashSet<NotebookEntryDataSO>();
    public bool IsDiscovered(NotebookEntryDataSO entry) => entry != null && discovered.Contains(entry);

    // Returns false if the clue was already known (handy for "New clue!" popups)
    public bool Discover(NotebookEntryDataSO clue) => discovered.Add(clue);

    
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Pause the game by setting time scale to 0
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Resume the game by setting time scale back to 1
    }
    
    public bool IsGamePaused()
    {
        return isGamePaused;
    }
}
