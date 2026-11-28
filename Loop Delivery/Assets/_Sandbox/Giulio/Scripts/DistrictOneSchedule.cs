using System;
using UnityEngine;

namespace LoopDelivery.Giulio
{
    [CreateAssetMenu(menuName = "Loop Delivery/District One Schedule")]
    public sealed class DistrictOneSchedule : ScriptableObject
    {
        [Serializable]
        public struct Window
        {
            [SerializeField] private GameTime opens;
            [SerializeField] private GameTime closes;

            public Window(uint openHour, uint openMinute, uint closeHour, uint closeMinute)
            {
                opens = new GameTime(openHour, openMinute, 0);
                closes = new GameTime(closeHour, closeMinute, 0);
            }

            public uint Opens => opens.ToTotalSeconds();
            public uint Closes => closes.ToTotalSeconds();
            public bool Contains(uint time) => time >= Opens && time < Closes;
            public string Display => $"{DistrictOneSession.FormatTime(Opens)}–{DistrictOneSession.FormatTime(Closes)}";
        }

        [SerializeField] private Window bakery = new(7, 0, 11, 0);
        [SerializeField] private Window bookshop = new(7, 0, 11, 0);
        [SerializeField] private Window townHall = new(11, 0, 13, 0);
        [SerializeField] private Window marketPassage = new(7, 0, 9, 25);
        [SerializeField] private Window canalUnloading = new(10, 0, 10, 35);
        [Min(0)] [SerializeField] private int arrivalWaitMinutes = 35;

        public Window MarketPassage => marketPassage;
        public Window CanalUnloading => canalUnloading;
        public bool CanWait(uint now, uint opens) => now < opens && opens - now <= arrivalWaitMinutes * 60L;

        public Window Counter(int index) => index switch
        {
            0 => bakery,
            1 => bookshop,
            2 => townHall,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
