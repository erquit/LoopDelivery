using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Layout = LoopDelivery.Giulio.DistrictOneLayout;
using Surface = LoopDelivery.Giulio.DistrictOneLayout.Surface;
using Interaction = LoopDelivery.Giulio.DistrictOneStop.Action;

namespace LoopDelivery.Giulio
{
    public sealed class DistrictOneSceneBuilder
    {
        private const string Sandbox = "Assets/_Sandbox/Giulio/";
        private const string Art = Sandbox + "Art/";
        private const string Common = "Assets/_Game/";
        private const string Figma = Common + "Sprites/FigmaImports/";
        public const string ScenePath = Sandbox + "DistrictOneRouteLab.unity";

        private readonly List<DistrictOneStop> stops = new();
        private readonly Dictionary<string, Sprite> sprites = new();
        private Material spriteMaterial;
        private Font signFont;
        private Transform streets;
        private Transform interior;
        private GameObject[] shutters;
        private GameObject unloadingSign;
        private GameObject restingSign;
        private Transform barge;
        private DistrictOneSchedule schedule;
        private BoxCollider2D marketBarrier;

        [MenuItem("Tools/Loop Delivery/Rebuild District One Route Lab")]
        public static void Build()
        {
            Scene previous = SceneManager.GetActiveScene();
            if (SceneManager.GetSceneByPath(ScenePath).isLoaded)
                throw new InvalidOperationException("Close the Route Lab scene before rebuilding it.");

            Scene generated = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(generated);
            try
            {
                new DistrictOneSceneBuilder().WriteScene(generated);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(generated, true);
            }
        }

        private void WriteScene(Scene scene)
        {
            PrepareSprites();
            schedule = LoadSchedule();
            streets = new GameObject("Oakhaven · District 1").transform;
            interior = new GameObject("Oakhaven Express · Depot interior").transform;
            interior.position = Layout.Interior;
            signFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            BuildSurface();
            BuildLandmarks();
            AddStreetDressing();
            BuildInterior();

            GameObject clock = Prefab(Common + "Prefabs/ClockManager/Global Clock Manager.prefab");
            clock.name = "Global Clock Manager";
            clock.GetComponent<GlobalClockManager>().realLoopTimeInMinutes = 4;
            clock.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
            GameObject player = Prefab(Common + "Prefabs/Player.prefab");
            player.name = "Courier (walking)";
            player.GetComponent<PlayerInteractor>().enabled = false;
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<PlayerInput>().enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var playerRenderer = player.GetComponent<SpriteRenderer>();
            var animator = player.GetComponent<Animator>();
            animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(
                Common + "Animations/FigmaImports/PlayerAnimator_FigmaCourier.overrideController");
            playerRenderer.sprite = sprites["walk_down_idle"];
            playerRenderer.sharedMaterial = spriteMaterial;
            playerRenderer.sortingOrder = 18;

            Vector2 spawn = Layout.Interior + new Vector2(0, -3.4f);
            player.transform.position = spawn;
            GameObject cameraObject = new("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = (Vector3)DistrictOneSession.DepotCameraCenter + Vector3.back * 10;
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = DistrictOneSession.DepotCameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(24, 34, 39, 255);
            cameraObject.AddComponent<AudioListener>();

            string[] parcelNames = { "PackageToBackery", "PackageToBookshop", "PackageToTownHall" };
            ParcelDataSO[] parcels = parcelNames.Select(name =>
                Load<ParcelDataSO>(Common + $"Data/Parcels/{name}.asset")).ToArray();
            GameObject systems = new("District 1 · session and HUD");
            var streetEvents = streets.gameObject.AddComponent<DistrictOneStreetEvents>();
            streetEvents.Configure(marketBarrier, unloadingSign, restingSign, barge);
            systems.AddComponent<DistrictOneSession>().Configure(body, animator,
                player.GetComponent<ParcelInventory>(), camera, parcels, stops.ToArray(), shutters,
                schedule, streetEvents, spawn, Layout.Depot);
            systems.AddComponent<DistrictOneHud>();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save the Giulio District 1 scene.");
            AssetDatabase.SaveAssets();
            Debug.Log($"District 1 scene saved with {stops.Count} interaction points.");
        }

        private static DistrictOneSchedule LoadSchedule()
        {
            const string path = Sandbox + "DistrictOneSchedule.asset";
            var settings = AssetDatabase.LoadAssetAtPath<DistrictOneSchedule>(path);
            if (settings != null)
                return settings;

            settings = ScriptableObject.CreateInstance<DistrictOneSchedule>();
            AssetDatabase.CreateAsset(settings, path);
            return settings;
        }

        private void PrepareSprites()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Art.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool grass = path == Art + "Verge Grass.png";
                if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 16 ||
                    importer.filterMode != FilterMode.Point || importer.mipmapEnabled ||
                    importer.textureCompression != TextureImporterCompression.Uncompressed ||
                    importer.spriteImportMode != SpriteImportMode.Single || grass && importer.wrapMode != TextureWrapMode.Repeat)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 16;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    if (grass)
                        importer.wrapMode = TextureWrapMode.Repeat;
                    importer.SaveAndReimport();
                }

                Sprite sprite = Load<Sprite>(path);
                sprites.Add(sprite.name, sprite);
            }
            foreach (string sheet in new[] { "Buildings_16x16.png", "Courier_SpriteSheet.png" })
            {
                foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(Figma + sheet).OfType<Sprite>())
                    sprites.Add(sprite.name, sprite);
            }

            string materialPath = Art + "District Sprite.mat";
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (spriteMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null)
                    throw new InvalidOperationException("The project's URP 2D sprite shader is missing.");
                spriteMaterial = new Material(shader);
                AssetDatabase.CreateAsset(spriteMaterial, materialPath);
            }
        }

        private void BuildSurface()
        {
            BuildGroundBackdrop();
            GameObject grid = new("Street tiles", typeof(Grid));
            grid.transform.SetParent(streets, false);
            Tilemap ground = MapLayer(grid.transform, "Roads, walks and canal", -10);
            Tilemap verge = MapLayer(grid.transform, "Grass verges", -11);
            Dictionary<Surface, Tile> tiles = new()
            {
                { Surface.Paving, TileFor("Road Paving") },
                { Surface.Asphalt, TileFor("Road Asphalt") },
                { Surface.Brick, TileFor("Arcade Brick", new Color32(191, 166, 142, 255)) },
                { Surface.Plaza, TileFor("Square Paving") },
                { Surface.Stone, TileFor("Steps Stone") },
                { Surface.Wood, TileFor("Bridge Deck") },
                { Surface.Water, TileFor("Canal Water") },
                { Surface.Market, TileFor("Market Stone", "Steps Stone", new Color32(209, 180, 172, 255)) }
            };
            Tile waterVar = TileFor("Canal Water Var");
            Tile grass = TileFor("Verge Grass");
            int width = Layout.StreetMax.x - Layout.StreetMin.x;
            int height = Layout.StreetMax.y - Layout.StreetMin.y;
            var floor = new Surface[width, height];
            var cells = new List<Vector3Int>();
            var surfaces = new List<TileBase>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new Vector3Int(Layout.StreetMin.x + x, Layout.StreetMin.y + y, 0);
                    Surface surface = Layout.FloorAt(new Vector2(cell.x + 0.5f, cell.y + 0.5f));
                    floor[x, y] = surface;
                    if (surface == Surface.Ground)
                        continue;
                    cells.Add(cell);
                    surfaces.Add(surface == Surface.Water && (x * 173 + y * 271) % 13 == 0 ? waterVar : tiles[surface]);
                }
            }
            ground.SetTiles(cells.ToArray(), surfaces.ToArray());
            cells.Clear();
            surfaces.Clear();
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (floor[x, y] != Surface.Ground)
                        continue;
                    bool besideRoad = false;
                    for (int delta = 1; delta <= 3 && !besideRoad; delta++)
                    {
                        besideRoad = x >= delta && x + delta < width && y >= delta && y + delta < height &&
                            (floor[x - delta, y] != Surface.Ground || floor[x + delta, y] != Surface.Ground ||
                             floor[x, y - delta] != Surface.Ground || floor[x, y + delta] != Surface.Ground);
                    }
                    if (!besideRoad)
                        continue;
                    cells.Add(new Vector3Int(Layout.StreetMin.x + x, Layout.StreetMin.y + y, 0));
                    surfaces.Add(grass);
                }
            }
            verge.SetTiles(cells.ToArray(), surfaces.ToArray());
            AddStreetBoundaries(floor);
        }

        private void BuildGroundBackdrop()
        {
            string path = Art + "District Ground.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            Vector2 size = Layout.StreetMax - Layout.StreetMin + new Vector2(40, 40);
            material.SetTexture("_BaseMap", sprites["Verge Grass"].texture);
            material.SetTextureScale("_BaseMap", size);
            material.SetColor("_BaseColor", new Color32(195, 206, 177, 255));
            EditorUtility.SetDirty(material);

            GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "Meadow ground";
            backdrop.transform.SetParent(streets, false);
            backdrop.transform.localPosition = (Vector2)(Layout.StreetMin + Layout.StreetMax) * 0.5f;
            backdrop.transform.localPosition += Vector3.forward * 2;
            backdrop.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = backdrop.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -20;
            UnityEngine.Object.DestroyImmediate(backdrop.GetComponent<Collider>());
        }

        private Tilemap MapLayer(Transform grid, string name, int order)
        {
            GameObject layer = new(name, typeof(Tilemap), typeof(TilemapRenderer));
            layer.transform.SetParent(grid, false);
            var renderer = layer.GetComponent<TilemapRenderer>();
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = order;
            return layer.GetComponent<Tilemap>();
        }

        private Tile TileFor(string name, Color? tint = null) => TileFor(name, name, tint);

        private Tile TileFor(string name, string sprite, Color? tint)
        {
            string path = Art + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprites[sprite];
            tile.color = tint ?? Color.white;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private void AddStreetBoundaries(Surface[,] floor)
        {
            Transform boundaries = new GameObject("Terrain boundaries").transform;
            boundaries.SetParent(streets, false);
            var pending = new Dictionary<(int left, int right), int>();
            for (int y = 0; y <= floor.GetLength(1); y++)
            {
                var spans = new HashSet<(int left, int right)>();
                if (y < floor.GetLength(1))
                {
                    int start = -1;
                    for (int x = 0; x <= floor.GetLength(0); x++)
                    {
                        bool blocked = x < floor.GetLength(0) && floor[x, y] is Surface.Ground or Surface.Water;
                        if (blocked && start < 0)
                            start = x;
                        if (!blocked && start >= 0)
                        {
                            spans.Add((start, x));
                            start = -1;
                        }
                    }
                }
                foreach (var entry in pending.ToArray())
                {
                    if (spans.Remove(entry.Key))
                        continue;
                    int left = entry.Key.left;
                    int right = entry.Key.right;
                    int bottom = entry.Value;
                    Vector2 position = Layout.StreetMin + new Vector2((left + right) / 2f, (bottom + y) / 2f);
                    Box(boundaries, "Terrain", position, new Vector2(right - left, y - bottom));
                    pending.Remove(entry.Key);
                }
                foreach (var span in spans)
                    pending.Add(span, y);
            }
        }

        private void BuildLandmarks()
        {
            Building("Depot exterior", sprites["Depot Exterior"], Layout.DepotDoor + new Vector2(0, 0.9f), 1.08f);
            Posted(streets, "OAKHAVEN EXPRESS", Layout.DepotDoor + new Vector2(0, 2.25f));
            Box(streets, "Depot facade", Layout.DepotDoor + new Vector2(0, 1.75f), new Vector2(7, 1.6f));
            Stop(streets, Interaction.Depot, Layout.DepotDoor, radius: 2.1f);
            SpriteAt(streets, "Depot postbox", "Postbox", new Vector2(2.5f, 3.25f), 1.1f, 12);
            SpriteAt(streets, "Locked bicycle", "Locked Bike", new Vector2(7.1f, -0.3f), 1.2f);
            Stop(streets, Interaction.BikeRack, new Vector2(7.1f, -0.3f), radius: 1.55f);
            Sign(streets, "↑  Bakery   ·   Fountain Square", new Vector2(-0.3f, 4.4f), 0.28f);

            Building("Bakery", sprites["ShoppingCenter_2"], Layout.Bakery + new Vector2(0, 1.4f), 1.1f);
            Box(streets, "Bakery facade", Layout.Bakery + new Vector2(-0.8f, 2.5f), new Vector2(6.5f, 1.65f));
            Stop(streets, Interaction.Bakery, Layout.Bakery, radius: 2.1f);
            Posted(streets, "BAKERY", Layout.Bakery + new Vector2(0, 2.3f));
            Sign(streets, "←  Bakery   ·   ↑  Market Street", new Vector2(0, 32.8f), 0.29f);
            Sign(streets, "Market Street  →", new Vector2(-2.7f, 61.1f), 0.29f);

            Building("Bookshop", sprites["ShoppingCenter_3"], Layout.Bookshop + new Vector2(0, 2.1f), 1.07f * 122 / 128);
            Box(streets, "Bookshop facade", Layout.Bookshop + new Vector2(0, 3.1f), new Vector2(7.5f, 1.6f));
            Stop(streets, Interaction.Bookshop, Layout.Bookshop, radius: 2.2f);
            Posted(streets, "IVO’S BOOKSHOP", Layout.Bookshop + new Vector2(0, 2.3f));
            Sign(streets, "↑  Clock Steps", new Vector2(46.6f, 66.1f), 0.25f);
            Sign(streets, "↓  Fountain Square", new Vector2(32.8f, 60.7f), 0.25f);
            Sign(streets, "Canal Bridge  →", new Vector2(58.5f, 65.8f), 0.25f);
            Sign(streets, "↓  Printers’ Arcade", new Vector2(52.6f, 59.7f), 0.25f);

            Building("Town Hall", sprites["GenericBuilding_2"], Layout.TownHall + new Vector2(0, 2), 0.97f * 220 / 232);
            Box(streets, "Town Hall facade", Layout.TownHall + new Vector2(0, 3), new Vector2(11.5f, 1.65f));
            Stop(streets, Interaction.TownHall, Layout.TownHall, radius: 2.05f);
            Posted(streets, "TOWN HALL", Layout.TownHall + new Vector2(0, 2.2f));
            Sign(streets, "Canal Bridge  →", Layout.TownHall + new Vector2(15, -2.8f), 0.28f);
            Sign(streets, "PRINTERS’ DISPATCH", Layout.TownHall + new Vector2(8, 3.1f), 0.24f);
            Stop(streets, Interaction.DispatchPlate, Layout.TownHall + new Vector2(8, 1.2f), radius: 1.9f);

            Vector2 plaza = Layout.Square;
            SpriteAt(streets, "Square fountain", "Fountain", plaza + new Vector2(-2.2f, 2.3f), 0.96f);
            Box(streets, "Fountain basin", plaza + new Vector2(-2.2f, 1.8f), new Vector2(2.65f, 2.25f));
            SpriteAt(streets, "West bench", "Plaza Bench", plaza + new Vector2(-7.1f, 2.9f), 0.9f);
            SpriteAt(streets, "Coffee kiosk", "Coffee Kiosk", plaza + new Vector2(8.6f, 5.1f), 0.59f);
            Box(streets, "Coffee kiosk counter", plaza + new Vector2(8.6f, 3.4f), new Vector2(2.6f, 1.25f));
            SpriteAt(streets, "Square pigeon", "Pigeon", plaza + new Vector2(1.3f, -1), 0.47f);
            SpriteAt(streets, "Square pigeon", "Pigeon", plaza + new Vector2(1.88f, -1.32f), 0.41f).flipX = true;
            Posted(streets, "FOUNTAIN SQUARE", plaza + new Vector2(-2.2f, -1.5f));
            Sign(streets, "↑  Bookshop   ·   Town Hall  →", plaza + new Vector2(6.2f, -3.4f), 0.27f);
            Sign(streets, "↑  Bakery   ·   Fountain Square  →", new Vector2(-0.5f, 13.5f), 0.27f);
            Sign(streets, "↑  Town Hall", new Vector2(83, 14.1f), 0.29f);

            Posted(streets, "CLOCK STEPS", Layout.Steps + new Vector2(0, 2.4f));
            Sign(streets, "CANAL TIMETABLE", Layout.Steps + new Vector2(0, 1.5f), 0.25f);
            Stop(streets, Interaction.Timetable, Layout.Steps, radius: 2.05f);
            for (int i = 0; i < 5; i++)
                TintSprite(streets, "Steps tread", new Vector2(45, 73 + i * 0.58f), new Vector2(5.9f, 0.11f),
                    new Color32(126, 137, 139, 255), -5);

            shutters = new[]
            {
                Shutter(Layout.ShopShutter, false, Interaction.ShopShutter),
                Shutter(Layout.HallShutter, true, Interaction.HallShutter)
            };
            BuildMarketGate();
            BuildCanal();
        }

        private void BuildMarketGate()
        {
            GameObject barrier = Child(streets, "Market passage barriers", Layout.MarketGate);
            marketBarrier = barrier.AddComponent<BoxCollider2D>();
            marketBarrier.size = new Vector2(0.65f, Layout.Market.Width + 0.2f);
            for (int i = 0; i < 4; i++)
                SpriteAt(barrier.transform, "Market barrier", "Market Barrier",
                    new Vector2(0, -2.55f + i * 1.68f), 0.77f, 8);
            barrier.SetActive(!schedule.MarketPassage.Contains(7 * 3600));

            Posted(streets, "MARKET PASSAGE", Layout.MarketGate + new Vector2(0, 4.7f));
            Sign(streets, $"Through route until {DistrictOneSession.FormatTime(schedule.MarketPassage.Closes)}",
                Layout.MarketGate + new Vector2(0, 3.85f), 0.25f);
            Stop(streets, Interaction.MarketGate, Layout.MarketGate, radius: 2.4f);
            SpriteAt(streets, "Ice cream stall", "Ice Cream Kiosk", Layout.MarketGate + new Vector2(7, 6.1f), 0.62f);
            SpriteAt(streets, "Market bench", "Plaza Bench", Layout.MarketGate + new Vector2(-6, 4.25f), 0.9f);
            Sign(streets, "Canal Bridge  ↓", new Vector2(109.8f, 65.1f), 0.26f);
            Sign(streets, "↑  Market passage   ·   Canal Bridge  →", new Vector2(110, 27.1f), 0.26f);
        }

        private void BuildCanal()
        {
            Vector2 canal = Layout.Canal;
            Posted(streets, "CANAL BRIDGE", new Vector2(116.5f, 34.6f));
            SpriteAt(streets, "Crates on the pier", "Pier Crates", canal + new Vector2(1.4f, 2.5f), 1.0f);
            SpriteAt(streets, "Loading parcel", "Dock Parcel", canal + new Vector2(-1.5f, 2.15f), 0.75f);
            SpriteAt(streets, "Harbour crew", "Beppe", canal + new Vector2(-0.6f, 1.45f), 0.85f)
                .color = new Color32(238, 206, 165, 255);
            barge = SpriteAt(streets, "Canal barge", "Canal Barge", new Vector2(123, 24), 1.38f, -3).transform;
            Transform board = Child(streets, "Loading board", canal + new Vector2(0.4f, 3.65f)).transform;
            unloadingSign = Child(board, "Address during unloading", Vector2.zero);
            Sign(unloadingSign.transform, "MERIDIAN HOUSE", Vector2.zero, 0.34f);
            unloadingSign.SetActive(false);
            restingSign = Child(board, "Covered address", Vector2.zero);
            Sign(restingSign.transform, "HARBOUR LOADING", Vector2.zero, 0.32f);
            Stop(streets, Interaction.Canal, canal, radius: 2.25f);
            TintSprite(streets, "North pier rail", new Vector2(123, 33.3f), new Vector2(14.5f, 0.29f),
                new Color32(118, 78, 49, 255), 2);
            TintSprite(streets, "South pier rail", new Vector2(123, 26.6f), new Vector2(14.5f, 0.29f),
                new Color32(118, 78, 49, 255), 2);
        }

        private GameObject Shutter(Vector2 position, bool vertical, Interaction action)
        {
            var shutter = new GameObject("Closed arcade shutter");
            shutter.transform.SetParent(streets, false);
            shutter.transform.position = position;
            Vector2 size = vertical ? new Vector2(0.58f, 6.2f) : new Vector2(6.2f, 0.58f);
            var collider = shutter.AddComponent<BoxCollider2D>();
            collider.size = size;
            TintSprite(shutter.transform, "Steel gate", Vector2.zero, size, new Color32(51, 49, 58, 255), 4);
            SpriteAt(shutter.transform, "Keep clear", "Bollard", Vector2.zero, 0.8f, 5);
            Sign(streets, "PRINTERS’ ARCADE", position + new Vector2(0, 2.8f), 0.25f);
            Stop(streets, action, position, radius: 2.45f);
            return shutter;
        }

        private void AddStreetDressing()
        {
            const string path = Art + "District One Street Dressing.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                GameObject root = new("District One Street Dressing");
                BuildStreetFurniture(root.transform);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                UnityEngine.Object.DestroyImmediate(root);
                if (prefab == null)
                    throw new InvalidOperationException("Could not create the street dressing prefab.");
            }

            PrefabUtility.InstantiatePrefab(prefab, streets);
        }

        private void BuildStreetFurniture(Transform dressing)
        {
            foreach (float y in new[] { 9f, 22f, 48f, 55f })
            {
                Building(dressing, "West brick frontage", sprites["GenericBuilding_4"], new Vector2(-7.7f, y), 0.74f);
                Building(dressing, "East brick frontage", sprites["GenericBuilding_3"], new Vector2(8.4f, y + 2), 0.67f, bottomPaddingPixels: 16);
            }
            foreach (float x in new[] { 13f, 21f, 27f, 76f, 84f, 92f, 101f })
            {
                Building(dressing, "Market Street shopfront", sprites["GenericBuilding_4"], new Vector2(x, 67.4f), 0.72f);
                SpriteAt(dressing, "Shop planters", "Flower Pot", new Vector2(x - 2.4f, 66.4f), 0.76f);
            }
            foreach (float x in new[] { 46f, 55f, 64f, 73f })
            {
                Building(dressing, "Square approach frontage", sprites["GenericBuilding_3"], new Vector2(x, 22.6f), 0.66f, bottomPaddingPixels: 16);
                SpriteAt(dressing, "Square approach tree", "Street Tree", new Vector2(x, 10.7f), 0.9f);
            }
            foreach (Vector2 point in new Vector2[]
                     { new(-3.9f, 6), new(4.2f, 28), new(-4.6f, 43), new(16, 59.4f), new(26, 60),
                       new(22, 15), new(34, 23), new(60, 12.6f), new(78.2f, 28), new(104, 32.8f), new(113.8f, 61) })
                SpriteAt(dressing, "Blue street lantern", "Blue Streetlight", point, 0.86f);

            foreach (Vector2 point in new Vector2[]
                     { new(26.5f, 33), new(26.5f, 40), new(27.5f, 49), new(42, 37), new(44, 46),
                       new(46, 52), new(93, 36), new(99, 36.7f), new(106, 42), new(104, 51),
                       new(55, 84.1f), new(40, 84.1f), new(-8, 57), new(137, 35) })
                SpriteAt(dressing, "Neighbourhood tree", "Street Tree", point, 0.93f, 1);
        }

        private void BuildInterior()
        {
            SpriteAt(interior, "Post office floor and counter", "Depot Interior", Vector2.zero, 1.36f, 0);
            SpriteAt(interior, "Double doors", "Depot Door", new Vector2(0, -7.25f), 1.31f, 1);
            SpriteAt(interior, "Beppe behind the counter", "Beppe", new Vector2(-7.05f, 1.5f), 1.2f, 6);
            SpriteAt(interior, "Locker #0000", "Parcel Locker", new Vector2(6.75f, 1.45f), 1.42f, 6);
            SpriteAt(interior, "Unclaimed parcels", "Parcels", new Vector2(8.3f, 0.92f), 0.9f, 7);
            Posted(interior, "BEPPE · DISPATCH", new Vector2(-6.8f, -1.2f));
            Posted(interior, "LOCKER #0000", new Vector2(6.65f, 2.75f));
            Posted(interior, "OAKHAVEN EXPRESS DEPOT", new Vector2(0, 5.9f), 0.38f);
            Sign(interior, "EXIT ↓", new Vector2(0, -5.72f), 0.32f);

            Box(interior, "Front wall", new Vector2(0, -5.25f), new Vector2(22.6f, 0.5f));
            Box(interior, "Back wall", new Vector2(0, 2.2f), new Vector2(22.6f, 0.6f));
            Box(interior, "Left wall", new Vector2(-10.4f, -1.6f), new Vector2(0.55f, 7.5f));
            Box(interior, "Right wall", new Vector2(10.4f, -1.6f), new Vector2(0.55f, 7.5f));
            Box(interior, "Dispatch counter", new Vector2(-7.15f, 0.67f), new Vector2(5.0f, 4.65f));
            Box(interior, "Parcel storage", new Vector2(8.2f, 1.17f), new Vector2(3.8f, 2.36f));
            Stop(interior, Interaction.Beppe, new Vector2(-6.8f, -1.72f), true, 2);
            Stop(interior, Interaction.Locker, new Vector2(6.6f, -0.15f), true, 2.1f);
            Stop(interior, Interaction.Exit, new Vector2(0, -4.8f), true, 1.15f);
        }

        private void Building(string name, Sprite sprite, Vector2 foot, float scale)
            => Building(streets, name, sprite, foot, scale);

        private void Building(Transform parent, string name, Sprite sprite, Vector2 foot, float scale,
            int bottomPaddingPixels = 0)
        {
            float footToCenter = sprite.bounds.extents.y - bottomPaddingPixels / sprite.pixelsPerUnit;
            Vector2 center = foot + Vector2.up * (footToCenter * scale);
            SpriteRenderer image = SpriteAt(parent, name, sprite, center, scale, 2);
            image.spriteSortPoint = SpriteSortPoint.Pivot;
        }

        private void Posted(Transform parent, string text, Vector2 position, float size = 0.34f)
        {
            Sign(parent, text, position, size);
        }

        private TextMesh Sign(Transform parent, string text, Vector2 position, float size = 0.33f)
        {
            float width = Mathf.Max(1.4f, text.Length * size * 0.49f + 0.55f);
            TintSprite(parent, text + " backing", position, new Vector2(width, size * 1.75f),
                new Color32(35, 58, 60, 255), 19);
            GameObject sign = Child(parent, text, position);
            TextMesh label = sign.AddComponent<TextMesh>();
            label.font = signFont;
            label.fontSize = 42;
            label.characterSize = size * 0.26f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color32(254, 232, 181, 255);
            label.text = text;
            MeshRenderer renderer = sign.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = signFont.material;
            renderer.sortingOrder = 20;
            return label;
        }

        private void TintSprite(Transform parent, string name, Vector2 position, Vector2 size, Color color, int order)
        {
            SpriteRenderer renderer = SpriteAt(parent, name, "Road Paving", position, 1, order);
            renderer.transform.localScale = size;
            renderer.color = color;
        }

        private SpriteRenderer SpriteAt(Transform parent, string name, string sprite, Vector2 position,
            float scale, int order = 3)
        {
            return SpriteAt(parent, name, sprites[sprite], position, scale, order);
        }

        private SpriteRenderer SpriteAt(Transform parent, string name, Sprite sprite, Vector2 position,
            float scale, int order)
        {
            var renderer = Child(parent, name, position).AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = order;
            renderer.transform.localScale = new Vector3(scale, scale, 1);
            return renderer;
        }

        private void Stop(Transform parent, Interaction action, Vector2 position, bool inside = false, float radius = 1.85f)
        {
            var stop = Child(parent, $"Interact · {action}", position).AddComponent<DistrictOneStop>();
            stop.Configure(action, inside, radius);
            stops.Add(stop);
        }

        private static void Box(Transform parent, string name, Vector2 position, Vector2 size)
        {
            Child(parent, name, position).AddComponent<BoxCollider2D>().size = size;
        }

        private static GameObject Child(Transform parent, string name, Vector2 position)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            return child;
        }

        private static GameObject Prefab(string path)
        {
            return (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(path));
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset != null ? asset : throw new InvalidOperationException($"Missing {typeof(T).Name} at {path}");
        }
    }
}
