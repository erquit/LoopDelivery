using UnityEngine;

namespace LoopDelivery.Giulio
{
    public sealed class DistrictOneStreetEvents : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D marketBarrier;
        [SerializeField] private GameObject unloadingSign;
        [SerializeField] private GameObject restingSign;
        [SerializeField] private Transform barge;
        [SerializeField] private Vector2 bargeHome;

        public bool MarketOpen => !marketBarrier.gameObject.activeSelf;

        public void Configure(BoxCollider2D barrier, GameObject unloading, GameObject resting, Transform boat)
        {
            marketBarrier = barrier;
            unloadingSign = unloading;
            restingSign = resting;
            barge = boat;
            bargeHome = boat.position;
        }

        public void Refresh(uint now, DistrictOneSchedule schedule, Rigidbody2D courier)
        {
            bool shouldOpen = schedule.MarketPassage.Contains(now);
            if (MarketOpen != shouldOpen)
            {
                // Let the courier clear the doorway when the barrier comes down.
                Vector2 offset = courier.position - (Vector2)marketBarrier.transform.position;
                Vector2 clearance = marketBarrier.size / 2 + Vector2.one * 0.65f;
                bool inDoorway = Mathf.Abs(offset.x) <= clearance.x && Mathf.Abs(offset.y) <= clearance.y;
                if (shouldOpen || !inDoorway)
                    marketBarrier.gameObject.SetActive(!shouldOpen);
            }

            bool unloading = schedule.CanalUnloading.Contains(now);
            if (unloadingSign.activeSelf != unloading)
            {
                unloadingSign.SetActive(unloading);
                restingSign.SetActive(!unloading);
            }

            float offsetAlongCanal = unloading
                ? Mathf.Sin(now * 0.035f) * 0.19f
                : now < schedule.CanalUnloading.Opens ? -4f : -7.5f;
            barge.position = bargeHome + Vector2.up * offsetAlongCanal;
        }
    }
}
