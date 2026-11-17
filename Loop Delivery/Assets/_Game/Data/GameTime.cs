using UnityEngine;

[System.Serializable]
public struct GameTime
{
    public uint hour;
    public uint minute;
    public uint second;

    public GameTime(uint hours, uint minutes, uint seconds)
    {
        this.hour = hours;
        this.minute = minutes;
        this.second = seconds;
    }

    public uint ToTotalSeconds()
    {
        return hour * 3600 + minute * 60 + second;
    }

}
