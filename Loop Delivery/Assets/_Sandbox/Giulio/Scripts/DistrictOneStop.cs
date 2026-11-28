using UnityEngine;

namespace LoopDelivery.Giulio
{
    public sealed class DistrictOneStop : MonoBehaviour
    {
        public enum Action
        {
            Beppe, Locker, Exit, Depot, Bakery, Bookshop, TownHall,
            Timetable, DispatchPlate, ShopShutter, HallShutter, Canal, BikeRack, MarketGate
        }

        [SerializeField] private Action action;
        [SerializeField] private bool inDepot;
        [SerializeField] private float reach = 1.85f;

        public Action Interaction => action;
        public bool InDepot => inDepot;
        public float Reach => reach;

        public void Configure(Action interaction, bool inside = false, float radius = 1.85f)
        {
            action = interaction;
            inDepot = inside;
            reach = radius;
        }
    }
}
