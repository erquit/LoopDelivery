using UnityEngine;

namespace LoopDelivery.Giulio
{
    public static class DistrictOneLayout
    {
        public static readonly Vector2Int StreetMin = new(-21, -14);
        public static readonly Vector2Int StreetMax = new(145, 97);
        public static readonly Vector2 Depot = new(0, 0);
        public static readonly Vector2 DepotDoor = new(5.7f, 3.5f);
        public static readonly Vector2 Bakery = new(-7, 36);
        public static readonly Vector2 Bookshop = new(36, 63);
        public static readonly Vector2 Square = new(30, 17);
        public static readonly Vector2 TownHall = new(83, 30);
        public static readonly Vector2 Steps = new(47, 82);
        public static readonly Vector2 Canal = new(131, 30);
        public static readonly Vector2 MarketGate = new(64, 63);
        public static readonly Vector2 ShopShutter = new(53, 53);
        public static readonly Vector2 HallShutter = new(65, 30);
        public const int CanalBank = 117;
        public const int FarBank = 129;
        public static readonly Vector2 Interior = new(-100, -20);

        public enum Surface { Ground, Paving, Asphalt, Brick, Plaza, Wood, Water, Stone, Market }

        public sealed class Street
        {
            public readonly Surface Surface;
            public readonly float Width;
            public readonly Vector2[] Points;

            public Street(Surface surface, float width, params Vector2[] points)
            {
                Surface = surface;
                Width = width;
                Points = points;
            }
        }

        public static readonly Street[] PublicStreets =
        {
            new(Surface.Asphalt, 8, Depot, new(0, 63), Bookshop),
            new(Surface.Brick, 6, new(0, 36), Bakery),
            new(Surface.Asphalt, 9, new(0, 17), Square, new(83, 17), TownHall),
            new(Surface.Asphalt, 8, Bookshop, new(36, 17)),
            new(Surface.Stone, 6, new(45, 63), new(45, 82), Steps),
            new(Surface.Asphalt, 9, TownHall, new(115, 30)),
            new(Surface.Wood, 7, new(115, 30), Canal)
        };

        public static readonly Street Market = new(Surface.Market, 7, Bookshop, new(110, 63), new(110, 30));
        public static readonly Street Arcade = new(Surface.Brick, 6, Bookshop, new(53, 63),
            new(53, 30), TownHall);

        // Map coordinates for the original scene while the team evaluates the Route Lab.
        internal static class Original
        {
            public static readonly Vector2Int StreetMin = new(-30, -20);
            public static readonly Vector2Int StreetMax = new(270, 280);
            public static readonly Vector2 Depot = new(0, 0);
            public static readonly Vector2 Bakery = new(-9, 188);
            public static readonly Vector2 Bookshop = new(100, 212);
            public static readonly Vector2 Square = new(35, 95);
            public static readonly Vector2 TownHall = new(145, 135);
            public static readonly Vector2 Steps = new(225, 262);
            public static readonly Vector2 Canal = new(231, 135);
    
            public static readonly Street[] PublicStreets =
            {
                new(Surface.Asphalt, 9, Depot, new(0, 212), Bookshop),
                new(Surface.Brick, 7, new(0, 188), Bakery),
                new(Surface.Asphalt, 9, new(0, 95), Square, new(145, 95), TownHall),
                new(Surface.Asphalt, 9, Bookshop, new(85, 212), new(85, 175), new(35, 175), Square),
                new(Surface.Stone, 8, Bookshop, new(93, 212), new(93, 262), Steps, new(225, 208),
                    new(207, 208), new(207, 169), new(154, 169), new(154, 135), TownHall),
                new(Surface.Asphalt, 9, TownHall, new(218, 135)),
                new(Surface.Wood, 9, new(218, 135), new(233, 135))
            };
    
            public static readonly Street Arcade = new(Surface.Brick, 6, Bookshop, new(110, 212),
                new(110, 135), TownHall);
        }

        public static Surface FloorAt(Vector2 point)
        {
            if (Mathf.Abs(point.x - Square.x) <= 11 && Mathf.Abs(point.y - Square.y) <= 7)
                return Surface.Plaza;
            if (Mathf.Abs(point.x - Depot.x) <= 10 && point.y >= -5 && point.y <= 6)
                return Surface.Plaza;
            if (Mathf.Abs(point.x - Steps.x) <= 5 && Mathf.Abs(point.y - Steps.y) <= 4)
                return Surface.Stone;

            for (int i = PublicStreets.Length - 1; i >= 0; i--)
            {
                Surface surface = Along(PublicStreets[i], point);
                if (surface != Surface.Ground)
                    return surface;
            }

            Surface arcade = Along(Arcade, point);
            if (arcade != Surface.Ground)
                return arcade;
            Surface market = Along(Market, point);
            if (market != Surface.Ground)
                return market;
            return point.x >= CanalBank && point.x < FarBank ? Surface.Water : Surface.Ground;
        }

        private static Surface Along(Street street, Vector2 point)
        {
            for (int i = 1; i < street.Points.Length; i++)
            {
                Vector2 a = street.Points[i - 1];
                Vector2 b = street.Points[i];
                float edge = street.Width / 2f;
                if (point.x < Mathf.Min(a.x, b.x) - edge || point.x >= Mathf.Max(a.x, b.x) + edge ||
                    point.y < Mathf.Min(a.y, b.y) - edge || point.y >= Mathf.Max(a.y, b.y) + edge)
                    continue;

                float center = a.x == b.x ? Mathf.Abs(point.x - a.x) : Mathf.Abs(point.y - a.y);
                return street.Surface == Surface.Asphalt && center >= edge - 2
                    ? Surface.Paving
                    : street.Surface;
            }
            return Surface.Ground;
        }
    }
}
