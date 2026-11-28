using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Action = LoopDelivery.Giulio.DistrictOneStop.Action;

namespace LoopDelivery.Giulio
{
    public sealed class DistrictOneSession : MonoBehaviour
    {
        private const string OriginalSaveKey = "LoopDelivery.Giulio.District1.v1";
        private const string RouteLabSaveKey = "LoopDelivery.Giulio.RouteLab.v1";
        private const float WalkSpeed = 5f;
        private const string Destination = "Meridian House";
        public const float DepotCameraSize = 8.1f;
        public static readonly Vector2 DepotCameraCenter = DistrictOneLayout.Interior + Vector2.down * 1.65f;

        public enum Screen { World, Message, Manifest, Map, Clues, Reset, Midnight, Complete, Pause }

        public static readonly string[] Items = { "Kitchen thermometer", "Street maps", "Replacement date wheel" };
        public static readonly string[] Recipients = { "Bakery", "Bookshop", "Town Hall" };
        public static readonly Vector2[] LandmarkPositions =
        {
            DistrictOneLayout.Bakery, DistrictOneLayout.Bookshop, DistrictOneLayout.TownHall,
            DistrictOneLayout.Depot, DistrictOneLayout.Square, DistrictOneLayout.Steps, DistrictOneLayout.Canal
        };
        public static readonly string[] LandmarkNames =
        {
            "Bakery", "Bookshop", "Town Hall", "Depot", "Fountain Square", "Clock Steps", "Canal Bridge"
        };
        private static readonly int Horizontal = Animator.StringToHash("HorizontalMovement");
        private static readonly int Vertical = Animator.StringToHash("VerticalMovement");
        private static readonly int PrevHorizontal = Animator.StringToHash("PreviousHorizontalMovement");
        private static readonly int PrevVertical = Animator.StringToHash("PreviousVerticalMovement");
        private static readonly int Moving = Animator.StringToHash("Moving");

        [Serializable]
        public sealed class Notebook
        {
            public bool fragment;
            public bool printerLead;
            public bool arcadeCode;
            public bool unloadingLead;
            public string unloadingSource;
            public bool oldAddress;
            public bool destination;
            public bool stampedShift;
            public int mornings;
        }

        [SerializeField] private Rigidbody2D courier;
        [SerializeField] private Animator courierAnimator;
        [SerializeField] private ParcelInventory inventory;
        [SerializeField] private Camera followCamera;
        [SerializeField] private ParcelDataSO[] parcels;
        [SerializeField] private DistrictOneStop[] stops;
        [SerializeField] private GameObject[] closedShutters;
        [SerializeField] private DistrictOneSchedule schedule;
        [SerializeField] private DistrictOneStreetEvents streetEvents;
        // Retained for the original sandbox scene.
        [SerializeField] private GameObject unloadingSign;
        [SerializeField] private GameObject restingSign;
        [SerializeField] private Transform barge;
        private Vector2 originalBargeHome;

        [SerializeField] private Vector2 indoorSpawn;
        [SerializeField] private Vector2 outsideSpawn;

        private readonly bool[] signed = new bool[3];
        private GlobalClockManager clock;
        private DistrictOneStop target;
        private Vector2 direction;
        private Vector2 facing = Vector2.down;
        private bool dispatched;
        private bool departed;
        private bool stampedToday;
        private int selection;
        private string extraAction;
        private uint waitUntil;

        public Notebook Notes { get; private set; }
        public Screen ActiveScreen { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string Notice { get; private set; }
        public float NoticeUntil { get; private set; }
        public bool InDepot { get; private set; } = true;
        public bool ShuttersOpen { get; private set; }
        public int Pin { get; private set; } = -1;
        public int Selection => selection;
        public string ExtraAction => extraAction;
        public DistrictOneSchedule Schedule => schedule;
        public bool IsRouteLab => streetEvents != null;
        public bool MarketOpen => IsRouteLab && streetEvents.MarketOpen;
        public Vector2 LandmarkPosition(int index)
        {
            if (IsRouteLab) return LandmarkPositions[index];
            return index switch
            {
                0 => DistrictOneLayout.Original.Bakery,
                1 => DistrictOneLayout.Original.Bookshop,
                2 => DistrictOneLayout.Original.TownHall,
                3 => DistrictOneLayout.Original.Depot,
                4 => DistrictOneLayout.Original.Square,
                5 => DistrictOneLayout.Original.Steps,
                _ => DistrictOneLayout.Original.Canal
            };
        }

        public string Objective => !dispatched ? "Collect parcels from Beppe"
            : SignedCount < Items.Length ? "Deliver the morning parcels"
            : !stampedToday ? "Return to Beppe for your stamp"
            : !Notes.destination ? "Investigate Locker #0000"
            : "Show Beppe the recovered address";
        public int SignedCount => (signed[0] ? 1 : 0) + (signed[1] ? 1 : 0) + (signed[2] ? 1 : 0);
        public uint Now => clock.GetCurrentGameTimeInSeconds();
        public string Phase => clock.GetCurrentTimeOfDay().ToString();
        public Vector2 Position => courier.position;
        public string Prompt => ActiveScreen == Screen.World && target != null ? Describe(target.Interaction) : "";

        public void Configure(Rigidbody2D body, Animator animator, ParcelInventory bag, Camera camera,
            ParcelDataSO[] manifest, DistrictOneStop[] interactions, GameObject[] shutters,
            DistrictOneSchedule timings, DistrictOneStreetEvents events, Vector2 inside, Vector2 outside)
        {
            courier = body;
            courierAnimator = animator;
            inventory = bag;
            followCamera = camera;
            parcels = manifest;
            stops = interactions;
            closedShutters = shutters;
            schedule = timings;
            streetEvents = events;
            indoorSpawn = inside;
            outsideSpawn = outside;
        }

        private void Awake()
        {
            clock = GlobalClockManager.Instance;
            Notes = ReadNotebook();
            if (schedule == null)
                schedule = ScriptableObject.CreateInstance<DistrictOneSchedule>();
            if (!IsRouteLab)
                originalBargeHome = barge.position;
            clock.ResetClock();
            ApplyClockState();
            MoveToDepot();
            RefreshStreetEvents(Now);
        }

        private void Update()
        {
            uint now = Now;
            RefreshStreetEvents(now);

            if (departed && now >= clock.gameClockEndHour * 3600 && ActiveScreen != Screen.Midnight)
            {
                Open(Screen.Midnight, "Midnight in Oakhaven",
                    $"Violet fog rolls inland from the canal. This shift ends with {SignedCount}/3 signatures.\n\nYour notebook goes with you into the next morning.");
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                direction = Vector2.zero;
                return;
            }

            direction = ActiveScreen == Screen.World ? ReadWalk(keyboard) : Vector2.zero;
            if (direction != Vector2.zero)
                facing = direction;
            if (ActiveScreen == Screen.World)
            {
                target = FindStop(now);
                ReadWorldInput(keyboard, now);
            }
            else
                ReadOverlayInput(keyboard);

            if (ActiveScreen != Screen.World)
                direction = Vector2.zero;
            Animate();
        }

        private void RefreshStreetEvents(uint now)
        {
            if (IsRouteLab)
            {
                streetEvents.Refresh(now, schedule, courier);
                return;
            }

            bool unloading = IsUnloading(now);
            unloadingSign.SetActive(unloading);
            restingSign.SetActive(!unloading);
            float offset = unloading ? 0.25f * (now / 16 % 4) : now < schedule.CanalUnloading.Opens ? 1.5f : 3.5f;
            barge.position = originalBargeHome + Vector2.down * offset;
        }

        private void FixedUpdate()
        {
            if (direction == Vector2.zero)
                return;
            courier.MovePosition(courier.position + direction * (WalkSpeed * Time.fixedDeltaTime));
        }

        private void LateUpdate()
        {
            Vector3 cameraPosition = InDepot ? DepotCameraCenter : courier.transform.position;
            cameraPosition.z = -10;
            followCamera.transform.position = cameraPosition;
        }

        private bool IsUnloading(uint now) => schedule.CanalUnloading.Contains(now);
        private bool IsOpen(int stop, uint now) => schedule.Counter(stop).Contains(now);

        public static string FormatTime(uint seconds)
        {
            return $"{seconds / 3600 % 24:00}:{seconds / 60 % 60:00}";
        }

        public string ParcelState(int stop)
        {
            if (signed[stop]) return "Signed";
            if (Now >= schedule.Counter(stop).Closes) return "Window missed";
            return dispatched ? "Carried" : "At the Depot";
        }

        private static Vector2 ReadWalk(Keyboard keyboard)
        {
            int x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
            int y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1);
        }

        private void Animate()
        {
            bool moving = direction != Vector2.zero;
            courierAnimator.SetBool(Moving, moving);
            if (moving)
            {
                courierAnimator.SetFloat(Horizontal, direction.x);
                courierAnimator.SetFloat(Vertical, direction.y);
            }
            else
            {
                courierAnimator.SetFloat(PrevHorizontal, facing.x);
                courierAnimator.SetFloat(PrevVertical, facing.y);
            }
        }

        private void ReadWorldInput(Keyboard keyboard, uint now)
        {
            if (keyboard.tabKey.wasPressedThisFrame)
                OpenNotebook(Screen.Manifest);
            else if (keyboard.mKey.wasPressedThisFrame)
                OpenNotebook(Screen.Map);
            else if (keyboard.escapeKey.wasPressedThisFrame)
                Open(Screen.Pause, "Paused", "Press Esc or E to return.");
            else if (keyboard.eKey.wasPressedThisFrame && target != null)
                Interact(target.Interaction, now);
        }

        private void ReadOverlayInput(Keyboard keyboard)
        {
            bool enter = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            bool close = keyboard.escapeKey.wasPressedThisFrame;
            bool interact = keyboard.eKey.wasPressedThisFrame;

            switch (ActiveScreen)
            {
                case Screen.Manifest:
                case Screen.Map:
                case Screen.Clues:
                    if (close || keyboard.tabKey.wasPressedThisFrame)
                        Close();
                    else if (keyboard.mKey.wasPressedThisFrame)
                    {
                        if (ActiveScreen == Screen.Map) Close();
                        else OpenNotebook(Screen.Map);
                    }
                    else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                    {
                        int delta = keyboard.leftArrowKey.wasPressedThisFrame ? -1 : 1;
                        int tab = ((int)ActiveScreen - (int)Screen.Manifest + delta + 3) % 3;
                        OpenNotebook((Screen)((int)Screen.Manifest + tab));
                    }
                    else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                    {
                        int delta = keyboard.upArrowKey.wasPressedThisFrame ? -1 : 1;
                        int rows = ActiveScreen == Screen.Map ? LandmarkNames.Length : Recipients.Length;
                        if (ActiveScreen != Screen.Clues)
                            selection = (selection + delta + rows) % rows;
                    }
                    else if (enter)
                    {
                        int place = ActiveScreen == Screen.Clues ? (Notes.unloadingLead ? 6 : -1) : selection;
                        if (place >= 0)
                            Pin = Pin == place ? -1 : place;
                    }
                    break;
                case Screen.Midnight:
                    if (interact || enter)
                        ResetMorning();
                    break;
                case Screen.Reset:
                    if (close || interact)
                        Close();
                    else if (enter)
                        ResetMorning();
                    break;
                default:
                    if (close || interact || (enter && extraAction == null))
                        Close();
                    else if (enter && waitUntil != 0)
                        WaitForOpening();
                    else if (enter)
                        Open(Screen.Reset, "Start another morning?",
                            "This shift’s parcels and signatures reset. Your notebook and completed shift stamp stay with you.");
                    break;
            }
        }

        private DistrictOneStop FindStop(uint now)
        {
            DistrictOneStop best = null;
            float bestScore = float.NegativeInfinity;
            foreach (DistrictOneStop stop in stops)
            {
                if (stop.InDepot != InDepot)
                    continue;
                Vector2 offset = (Vector2)stop.transform.position - Position;
                float distance = offset.magnitude;
                if (distance > stop.Reach)
                    continue;
                int delivery = DeliveryIndex(stop.Interaction);
                bool eligible = delivery >= 0 && dispatched && !signed[delivery] && IsOpen(delivery, now);
                float score = (eligible ? 10f : 0f) + Vector2.Dot(facing, offset.normalized) * 0.4f - distance;
                if (score <= bestScore)
                    continue;
                bestScore = score;
                best = stop;
            }
            return best;
        }

        private static int DeliveryIndex(Action action) => action switch
        {
            Action.Bakery => 0,
            Action.Bookshop => 1,
            Action.TownHall => 2,
            _ => -1
        };

        private string Describe(Action action)
        {
            int delivery = DeliveryIndex(action);
            if (delivery >= 0)
                return dispatched && !signed[delivery] && IsOpen(delivery, Now)
                    ? $"Deliver {Items[delivery].ToLowerInvariant()}"
                    : $"Talk at {Recipients[delivery]}";
            return action switch
            {
                Action.Beppe => "Talk to Beppe",
                Action.Locker => "Inspect Locker #0000",
                Action.Exit => "Leave the Depot",
                Action.Depot => "Enter the Depot",
                Action.DispatchPlate => "Read dispatch plate",
                Action.Timetable => "Read canal timetable",
                Action.Canal => IsUnloading(Now) ? "Copy the swinging address plate" : "Talk to the canal worker",
                Action.ShopShutter or Action.HallShutter => ShuttersOpen ? "Inspect the open arcade" : "Try the arcade shutter",
                Action.BikeRack => "Inspect the bicycle rack",
                Action.MarketGate => "Read the market gate sign",
                _ => ""
            };
        }

        private void Interact(Action action, uint now)
        {
            int delivery = DeliveryIndex(action);
            if (delivery >= 0)
            {
                Deliver(delivery, now);
                return;
            }

            switch (action)
            {
                case Action.Beppe:
                    TalkToBoss();
                    break;
                case Action.Exit:
                    Move(outsideSpawn, false);
                    departed = true;
                    ApplyClockState();
                    if (!dispatched)
                        Notify("Beppe has your parcels at the Depot.");
                    break;
                case Action.Depot:
                    MoveToDepot();
                    break;
                case Action.Locker:
                    InspectLocker();
                    break;
                case Action.DispatchPlate:
                    if (!Notes.arcadeCode)
                    {
                        Notes.arcadeCode = true;
                        RecordFact();
                    }
                    Open(Screen.Message, "Town Hall dispatch plate",
                        "PRINTERS’ ACCESS · 1842\nThe shutters connect the passage beside the Bookshop with Town Hall. The code is in your notebook.");
                    break;
                case Action.Timetable:
                    LearnSchedule("Clock Steps timetable");
                    Open(Screen.Message, "Canal timetable",
                        $"Barge unloading: {schedule.CanalUnloading.Display} at Canal Bridge. The crew rotates the address plate during unloading.");
                    break;
                case Action.Canal:
                    VisitCanal(now);
                    break;
                case Action.ShopShutter:
                case Action.HallShutter:
                    UseShutter();
                    break;
                case Action.MarketGate:
                    Open(Screen.Message, "Market passage", MarketOpen
                        ? $"The market passage is open until {FormatTime(schedule.MarketPassage.Closes)}. Follow it east to Canal Bridge."
                        : "The vendors are setting out their stalls. Take Fountain Square past Town Hall, or use the Printers’ Arcade if you know the code.");
                    break;
                case Action.BikeRack:
                    Open(Screen.Message, "Depot bicycle rack", "The bicycle is locked. The district’s streets and shortcuts are within walking distance.");
                    break;
            }
        }

        private void Deliver(int stop, uint now)
        {
            if (signed[stop])
            {
                Open(Screen.Message, Recipients[stop], "Your parcel is already signed for. Thank you!");
                return;
            }
            var window = schedule.Counter(stop);
            if (!window.Contains(now))
            {
                string opensAt = FormatTime(window.Opens);
                if (now < window.Opens)
                    ShowArrival(Recipients[stop], $"Counter opens at {opensAt}.", now, window.Opens);
                else
                    Open(Screen.Message, Recipients[stop], $"Back tomorrow at {opensAt}.");
                return;
            }
            if (!dispatched || inventory.DeliverTo(parcels[stop].destination) == 0)
            {
                Open(Screen.Message, Recipients[stop], "Pick up the manifest parcels from Beppe at the Depot.");
                return;
            }

            signed[stop] = true;
            if (stop == 1 && !Notes.printerLead)
            {
                Notes.printerLead = true;
                RecordFact();
            }
            string reply = stop switch
            {
                0 => IsRouteLab ? "Thanks! Follow Market Street east to Ivo’s Bookshop."
                    : "Thanks! Ivo’s Bookshop is north of here.",
                1 => "Ivo: The printers used that shutter next door. Their code is on the dispatch plate on Town Hall’s east wall.",
                _ => "Just in time to set the date wheel. Thank you, courier."
            };
            Open(Screen.Message, $"{Recipients[stop]} · receipt",
                $"{Items[stop]} signed for at {FormatTime(now)}.  {SignedCount}/3 delivered.\n\n{reply}");
        }

        private void TalkToBoss()
        {
            if (!dispatched)
            {
                foreach (ParcelDataSO parcel in parcels)
                    inventory.Add(parcel);
                dispatched = true;
                Open(Screen.Message, "Beppe · dispatch",
                    $"Here are three parcels to deliver! Good luck!\n\nThe Bakery and Bookshop take deliveries this morning. Town Hall opens at {FormatTime(schedule.Counter(2).Opens)}. You’ll find the windows in your notebook (Tab).");
                return;
            }

            if (SignedCount == parcels.Length && !stampedToday)
            {
                stampedToday = true;
                Notes.stampedShift = true;
                WriteNotebook();
                if (Notes.destination)
                    ShowComplete();
                else
                    Open(Screen.Message, "Beppe · timecard stamped",
                        "Congratulations! Your shift is over, no other deliveries for today.\n\nThree signatures, one good shift. Locker #0000 is still waiting for an address.",
                        "Start another morning");
                return;
            }

            if (Notes.stampedShift && Notes.destination)
            {
                ShowComplete();
                return;
            }
            Open(Screen.Message, "Beppe", stampedToday
                ? "Your shift is stamped. You can check Locker #0000 or take another look around town."
                : $"I already gave you the parcels. You have {SignedCount}/3 signatures so far.",
                departed ? "Start another morning" : null);
        }

        private void ShowComplete()
        {
            Open(Screen.Complete, "District 1 complete",
                $"A morning shift signed at all three stops and stamped by Beppe.\n\nParcel #0000’s destination recovered: {Destination}.",
                "Start another morning");
        }

        private void InspectLocker()
        {
            if (Notes.oldAddress)
            {
                if (!Notes.destination)
                {
                    Notes.destination = true;
                    RecordFact();
                }
                Open(Screen.Message, "Locker #0000",
                    $"The letters on the parcel match the bridge plate. You write {Destination} in the destination field."
                    + (Notes.stampedShift ? "\n\nShow Beppe the recovered address." : ""));
                return;
            }

            if (!Notes.fragment)
            {
                Notes.fragment = true;
                RecordFact();
            }
            Open(Screen.Message, "Locker #0000",
                "Most of the address label has faded. All you can read is “… House” and a small canal dispatch mark. The destination field is still blank.");
        }

        private void UseShutter()
        {
            if (!Notes.arcadeCode)
            {
                Open(Screen.Message, "Printers’ arcade", "A four-digit keypad locks the shutter. A brass plate reads TOWN HALL DISPATCH.");
                return;
            }
            if (ShuttersOpen)
            {
                Open(Screen.Message, "Printers’ arcade", "Both shutters are open for this morning.");
                return;
            }
            SetShutters(true);
            Notify("1842 accepted. Both arcade shutters are open.");
        }

        private void VisitCanal(uint now)
        {
            LearnSchedule("Canal Bridge worker");
            var window = schedule.CanalUnloading;
            if (!window.Contains(now))
            {
                if (now < window.Opens)
                    ShowArrival("Canal Bridge · harbour crew",
                        $"We unload {window.Display}. You can read the old address when we swing the plate around.",
                        now, window.Opens);
                else
                    Open(Screen.Message, "Canal Bridge · harbour crew",
                        $"We’re done for today. We unload {window.Display}."
                        + (IsRouteLab ? " Arrive via the early market passage or the Printers’ Arcade." : ""));
                return;
            }
            if (!Notes.oldAddress)
            {
                Notes.oldAddress = true;
                RecordFact();
            }
            Open(Screen.Message, "Canal Bridge · address plate",
                $"As the crew turns the plate, you copy the old address: {Destination}.\n\nCompare it with Parcel #0000 at the Depot.");
        }

        private void ShowArrival(string title, string message, uint now, uint opens)
        {
            if (!IsRouteLab || !schedule.CanWait(now, opens))
            {
                Open(Screen.Message, title, message);
                return;
            }

            Open(Screen.Message, title, message, $"Wait until {FormatTime(opens)}");
            waitUntil = opens;
        }

        private void WaitForOpening()
        {
            clock.TryAdvanceTo(waitUntil);
            RefreshStreetEvents(Now);
            Close();
            Notify($"It's {FormatTime(Now)}.");
        }

        private void LearnSchedule(string source)
        {
            if (Notes.unloadingLead)
                return;
            Notes.unloadingLead = true;
            Notes.unloadingSource = source;
            RecordFact();
        }

        private void ResetMorning()
        {
            foreach (ParcelDataSO parcel in parcels)
                inventory.DeliverTo(parcel.destination);
            Array.Clear(signed, 0, signed.Length);
            dispatched = false;
            departed = false;
            stampedToday = false;
            Pin = -1;
            Notes.mornings++;
            WriteNotebook();
            SetShutters(false);
            clock.ResetClock();
            MoveToDepot();
            RefreshStreetEvents(Now);
            Close();
            Notify("A new morning. Your notebook came with you.");
        }

        private void SetShutters(bool open)
        {
            ShuttersOpen = open;
            foreach (GameObject shutter in closedShutters)
                shutter.SetActive(!open);
        }

        private void MoveToDepot()
        {
            Move(indoorSpawn, true);
            ApplyClockState();
        }

        private void Move(Vector2 position, bool inside)
        {
            InDepot = inside;
            courier.position = position;
            courier.linearVelocity = Vector2.zero;
            direction = Vector2.zero;
            target = null;
            followCamera.orthographicSize = inside ? DepotCameraSize : 9f;
            followCamera.backgroundColor = inside ? new Color32(24, 34, 39, 255) : new Color32(80, 125, 71, 255);
        }

        private void ApplyClockState()
        {
            bool run = departed && !InDepot && ActiveScreen == Screen.World;
            if (run == clock.IsClockRunning)
                return;
            if (run) clock.StartClock();
            else clock.StopClock(roundToMinute: false);
        }

        private void OpenNotebook(Screen screen)
        {
            Open(screen, "Courier notebook", "");
            selection = screen == Screen.Manifest && Pin >= Recipients.Length ? 0 : Mathf.Max(Pin, 0);
        }

        private void Open(Screen screen, string title, string message, string optionalAction = null)
        {
            ActiveScreen = screen;
            Title = title;
            Message = message;
            extraAction = optionalAction;
            waitUntil = 0;
            ApplyClockState();
        }

        private void Close()
        {
            ActiveScreen = Screen.World;
            extraAction = null;
            waitUntil = 0;
            ApplyClockState();
        }

        private void RecordFact()
        {
            WriteNotebook();
            Notify("Copied to notebook");
        }

        private void Notify(string message)
        {
            Notice = message;
            NoticeUntil = Time.unscaledTime + 3f;
        }

        private string SaveKey => IsRouteLab ? RouteLabSaveKey : OriginalSaveKey;

        private Notebook ReadNotebook()
        {
            try
            {
                return JsonUtility.FromJson<Notebook>(PlayerPrefs.GetString(SaveKey, "")) ?? new Notebook();
            }
            catch (ArgumentException)
            {
                return new Notebook();
            }
        }

        private void WriteNotebook()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Notes));
            PlayerPrefs.Save();
        }
    }
}
