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

    public GameTime(uint totalSeconds)
    {
        this.hour = totalSeconds / 3600;
        this.minute = (totalSeconds % 3600) / 60;
        this.second = totalSeconds % 60;
    }

    public uint ToTotalSeconds()
    {
        return hour * 3600 + minute * 60 + second;
    }

    /**
     * next 3 functions are used for better dictionary key comparisons and to avoid duplicate keys in the subscribers dictionary
     */
    public override int GetHashCode()
    {
        return (int)(hour * 600 + minute * 60 + second);
    }

    public bool Equals(GameTime other)
    {
        return this.hour == other.hour && this.minute == other.minute && this.second == other.second;
    }

    public override bool Equals(object obj)
    {
        if (obj is GameTime other)
        {
            return Equals(other);
        }
        return false;
    }

}
