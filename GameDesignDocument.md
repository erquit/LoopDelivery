# Loop Delivery #

**Version:** 0.1 (Initial Draft - Version Zero)  
**Date:** October 31, 2026  

---

## Design History & Changelog ##

| Version | Date | Author(s) | GitHub Commit / Description of Changes |
| :--- | :--- | :--- | :--- |
| **0.1 (v0)** | 2026-10-31 | Andrii Lushchan, Bogdan Prunescu, Elif Tellal, Gabriele Bovo, Xiangyu Zhang | Initial Version 0.1 draft establishing core loop, 10-minute timer, story lore, 4 districts, and task schedule for the Nov 2 deadline. |

---

## Overview and vision statement ##

### Concept Statement ###
*Loop Delivery* is a 2D top-down pixel-art time-loop adventure and route-optimization game set in the seemingly cheerful town of Oakhaven. The player controls a summer courier who has 10 real-time minutes (07:00 to 00:00 in-game) to deliver packages, uncover hidden town secrets, and prevent a catastrophic midnight anomaly that resets the day.

### Genre & Target Audience ###
* **Genre:** 2D Top-Down Exploration / Time-Loop Puzzle & Route-Optimization Hybrid.
* **Target Audience:** University peers and fans of mystery-driven indie games (*Outer Wilds*, *Easy Delivery Co.* and *Last Day of June*,).
* **Target Platform:** PC (Windows, keyboard input).

### Unique Selling Points (USPs) ###
1. **Knowledge as Progression:** The physical world resets every 10 minutes at 00:00, but the player's knowledge (4-digit keypad codes, NPC schedules, hidden shortcuts) persists permanently in the Courier Notebook UI.
2. **Cozy Surface, Dark Mystery:** Blends relaxing package-delivery gameplay with an urgent investigation into why the town dies at midnight.
3. **Risk/Reward Route Optimization **(Prototype)**:** Shortcuts require strategic trade-offs—such as one-time-use keys or temporary movement speed debuffs—forcing players to plan the "Perfect Day" run.

---

## Gameplay ##

### Core Loop ###
1. **Wake Up (07:00 AM):** Spawn inside the Oakhaven Express Depot (timer paused). Check the Courier Notebook, talk to the Boss/Co-workers, and pick up daily parcels.
2. **Deliver & Explore (07:00 AM – 23:59 PM):** Exit the Depot (timer starts). Navigate the 4 districts, deliver parcels within NPC time windows, unlock shortcuts, and gather clues/codes.
3. **Midnight Reset (00:00 AM):** If the player is still in Oakhaven at 00:00, a violet fog rolls down from the Observatory, killing the player and resetting the day to 07:00 AM while preserving the Courier Notebook and discovered keypad codes.

### Time System ###
* **Loop Duration:** 10 minutes of real-world time equals 17 in-game hours (07:00 to 00:00).
* **Time Scale:** 1 real-world minute ≈ 1 hour and 42 minutes (102 minutes) of in-game time.
* **Safe Zone Pause:** The timer pauses completely while the player is inside the Courier Depot and while active dialogue or keypad UI popups are open.
* **Day Phases:** World lighting and NPC schedules update across 5 fixed phases:

| Phase | In-Game Time | Real-Time Duration | World & Gameplay Events |
| :--- | :--- | :--- | :--- |
| **Morning** | 07:00 – 11:00 | 2 min 21 sec | Shops open; ??? |
| **Midday** | 11:00 – 13:00 | 1 min 10 sec | ??? |
| **Afternoon** | 13:00 – 18:30 | 3 min 14 sec | ??? |
| **Dusk** | 18:30 – 20:00 | 0 min 53 sec | ??? |
| **Night** | 20:00 – 00:00 | 2 min 22 sec | Streetlights turn on, ???. At 00:00, lethal fog resets the loop. |

### Controls & Interfaces (UI) ###
* **Movement:** `W`, `A`, `S`, `D` or `Arrow Keys` (8-directional top-down movement).
* **Interact / Deliver / Talk:** `E` (context-sensitive interaction within 1 tile of an NPC, door, or keypad).
* **Open Courier Notebook / Map:** `Tab` or `M` (toggles persistent clue log and dynamic minimap).
* **Equip / Mount Transport:** `Spacebar` (toggles Skateboard or Bicycle once unlocked).
* **HUD Interface:** Displays top-right Digital Clock (`07:00`–`00:00`), top-left Active Parcel Counter, and bottom-right Minimap.

### Movement & Transportation Tiers ###
Transportation upgrades increase movement speed along existing roads and enable access to distance-gated zones:

| Tier | Item Name | Movement Speed | Unlock Method |
| :--- | :--- | :--- | :--- |
| **Tier 0** | Standard Walk | 100% (Base: ??? tiles/sec) | Default state at 07:00 AM. |
| **Tier 1** | Skateboard | 130% (??? tiles/sec) | Unlocked in District ??? by completing the ???. |
| **Tier 2** | Bicycle | 160% (??? tiles/sec) | Parked outside the Depot from Loop 1 behind a 4-digit padlock. Code is discovered in ???. |

### Shortcuts & Trade-Off Mechanics ###
1. **4-Digit Keypad Gates:** Require a 4-digit code discovered in another district. Once unlocked once, the player auto-inputs the code in 1 second on subsequent loops. **(Prototype)**
2. **Muddy Canal Path (Debuff Shortcut):** Cuts 90 seconds of travel between District 1 and District 2, but applies the *Muddy Boots* debuff, reducing walking speed by 30% for 10 real-time seconds. **(Prototype)**
3. **???**

---

## Characters (Prototype, work in progress) ##

1. **The Courier (Player Character):** A summer intern at Oakhaven Express. Retains memories across loops because they held **Parcel #0000** during the initial midnight blast.
2. **??? (The Depot Boss - Depot):** Hands out daily delivery manifests inside the Depot. Believes every loop is the player's first day on the job.
3. **Arthur (The Scientist / Watchmaker):** A recluse inside the Hilltop Observatory whose malfunctioning time machine trapped Oakhaven in a loop. Because his memory resets every morning, the player must bring him **Parcel #0000** before 00:00.

---

## Story ##

### Backstory & Premise ###
Oakhaven’s eccentric scientist, Arthur, built an experimental temporal device inside the Hilltop Observatory. However, the machine was activated without its core **Chronos Stabilizer**—a high-tech component stuck in transit inside an unclaimed box (**Parcel #0000**) at the Oakhaven Express Depot with a faded label. At 00:00, the machine malfunctioned, releasing a lethal fog that resets the town to 07:00 AM every day. Because the player was holding **Parcel #0000** when the shockwave hit, they are the only person in town who remembers previous loops.

### The Two Endings ###
* **Ending 1 — Normal Ending ("Clocking Out Early"):** Using the unlocked Bicycle and shortcuts, the player completes all standard daily deliveries across the town in a single loop and clocks out at the Depot before 00:00. The player leaves town early and survives, while Oakhaven resets behind them.
* **Ending 2 — True Ending ("The Final Delivery"):** Across multiple loops, the player uncovers ??? hidden clues that decode the faded address label on **Parcel #0000**. On the final "Perfect Day" run, the player unlocks Locker #0000, mounts the Bicycle, opens the shortcuts, and delivers the Chronos Stabilizer to Arthur at the Observatory before 00:00, fixing the machine and stopping the loop permanently.

---

## World (Prototype, work in progress) ##

The game world is a continuous 2D top-down tile-based map divided into **4 Districts**:

                   [ DISTRICT 4: Hilltop Observatory / Old Clocktower ]
                                            ^
                                            |
    [ DISTRICT 3: Residential Area ] <------+------> [ DISTRICT 2: Industrial Zone ]
                                            |       
                                            v
                         [ DISTRICT 1: Central Town & Depot ]
                              (Spawn Point & Locked Bike)

1. **District 1: Central Town & Depot (Starter Zone):** The main hub containing the Courier Depot (a safe zone where the timer pauses), the locked Bicycle, Parcel #0000, and the initial tutorial delivery locations.
2. **District 2: Industrial Zone:** A working-class area of warehouses and canals featuring mechanical obstacles, unlockable bridge shortcuts, and a muddy detour that slows the player down. **(Prototype)**
3. **District 3: Residential Area:** A quiet neighborhood of houses and narrow alleys where most townspeople live and follow their daily routines. **(Prototype)**
4. **District 4: Hilltop Observatory / Old Clocktower:** A steep, isolated path overlooking the town that leads to the Scientist's layer and the source of the midnight time loop.

## Media list ##

### Graphics (2D Pixel Art) ###
* **Tilesets (??? Palettes):** Central Plaza (warm cobblestone/brick), Industrial Docks (wood piers/water/metal), Old Quarter (dark stone/stained glass), Hilltop Woods (pine trees/observatory dome). **(Prototype)**
* **Character Spritesheets:** Player Courier (4-directional walk, Running Shoes, Skateboard, and Bicycle frames), ??? Key NPCs + ??? Generic Townspeople (2-frame idle animations). **(Prototype)**
* **Interactive Props:** Delivery boxes (Standard & Parcel #0000), 4-digit Padlock gate, doors (open/closed), Locked Bicycle.
* **UI Assets:** HUD Clock, Package Counter icon, Dynamic Minimap frame, Courier Notebook overlay, 4-Digit Keypad popup, Dialogue box. 
* **Color Palettes:** Warm colors for Central Plaza, blue-grey and rusty tones for Industrial Docks, earthy greens for the Residential Area, and darker tones for Hilltop Observatory to give each district its own atmosphere. **(Prototype)**
* **Environmental Sounds:** Background sounds that fit each district, such as everyday town sounds, industrial machinery, canal sounds, and nighttime forest ambience. **(Prototype)**

### Audio (Music & SFX) ###
* **Music Tracks (3 Tracks):** *Cozy Morning Delivery* (07:00–18:30), *Dusk Mystery* (18:30–23:00), *The Final Hour* (23:00–00:00). **(Prototype)**
* **Sound Effects (SFX):** Footsteps (stone/mud), Bicycle bell, Package delivered chime, Church bell toll (12:00), Keypad beep, Midnight fog reset sound. **(Prototype)**

---

## Technical specification (if applicable) ##

### Development Decisions & Architecture ###
* **Engine & Target Build:** Unity (2D Top-Down Orthographic camera **(Prototype)**, lighting tint for day/night phases).
* **Persistence Architecture (Save/Loop State):**
  * (Cleared at 00:00): Player position, current inventory of regular packages, temporary NPC positions, physical gate states.
  * (Saved across loops): Array of discovered clue with the notebook, unlocked 4-digit codes, and `LoopCounter` integer.
* **Time Manager System:** Global float `currentTime` ticking from (07:00) to (00:00) over `600` real seconds (10 minutes). Boolean flag `isTimePaused` sets time delta to `0` inside the Depot trigger volume and during active UI modals.

### Record of Key Design Decisions ###
1. **True Time-Loop Reset (Option A) vs. Multi-Day Survival (Option B):** We chose Option A (repeating the same day *Outer Wilds* style) because Option B required creating changing world assets for multiple days, which exceeds our 1-month development scope.
2. **10-Minute Real-Time Clock vs. Action-Based Time Slots (*Persona* style):** We chose a continuous 10-minute real-time clock (paused inside the Depot) rather than static time slots so that movement speed upgrades (Shoes/Skateboard/Bike) and route shortcuts directly impact gameplay urgency.
3. **Knowledge/Code Locks vs. Permanent World Changes:** Rather than keeping bridges physically lowered across loops, shortcuts use 4-digit keypads (such as the locked Bicycle outside the Depot). This keeps the time-loop logic consistent while letting players unlock the Bike in 5 seconds on subsequent loops once they learn the code.

---

## Team **(Prototype - Distibution of work in progress)** ## 

1. **Andrii Lushchan** (Team Leader/ Developer / Story, Game Design & Time/World Systems)
2. **Bogdan Prunescu** (Developer / Story, Loop Progression, Depot & Quest Logic)
3. **Elif Tellal** (Game Artist & Level Designer — Tilesets, Sprites & Map Layout)
4. **Gabriele Bovo** (Developer / Narrative Events, Endings & NPC Schedules) 
5. **Xiangyu Zhang** (Developer / Player Movement, Transport Tiers & Notebook UI)

---

## Deadlines ##

### Team Meeting 1 (October 26, 2026 — Brainstorming & Concept Selection) ###
- All team members contributed to the Figma board defining the loop structure, day phases, and game references (*Outer Wilds*, *Easy Delivery Co.*, *Last Day of June*).
- Agreed on Option A (Repeating Day Time-Loop) and a 10-minute loop duration with time paused inside the Depot.

### Milestone 0: Version Zero GDD Submission (Monday, November 2, 2026 @ 23:59) ###
- Complete and upload `GameDesignDocument.md` (Version 0.1) to the Git repository and submit the link via the course form (All Team Members).
- Finalize 4-district paper map layout and initial asset list (All Team Members).

### Week 1 (Sunday, November 8, 2026 deadline) (Prototype - Subjected to changes) ###
- Prototype core top-down movement (Walk + Bike speed tiers) and `E` interaction (Xiangyu Zhang, Andrii Lushchan).
- Prototype 10-minute Global Clock (07:00–00:00) with pause-in-Depot logic and 00:00 reset trigger (Bogdan Prunescu, Andrii Lushchan).
- Create District 1 (Central Town & Depot) greybox tileset and Courier sprite (Elif Tellal).

### Week 2 (Sunday, November 15, 2026 deadline) (Prototype - Subjected to changes) ###
- Implement Package Pickup/Delivery system and NPC time-bracket schedules (Morning/Midday/Afternoon/Dusk/Night) (Gabriele Bovo, Bogdan Prunescu).
- Implement Persistent Courier Notebook UI and 4-digit Keypad shortcut system (Xiangyu Zhang, Andrii Lushchan).
- Build District 2 (Industrial Zone) and District 3 (Residential Area) greybox maps with shortcut gates and Muddy Path debuff (Elif Tellal, All Devs).

### Week 3 (Sunday, November 22, 2026 deadline) (Prototype - Subjected to changes) ###
- Implement District 4 (Hilltop Observatory / Old Clocktower), Parcel #0000 questline, and both Endings (Normal & True Ending) (Gabriele Bovo, Bogdan Prunescu).
- Integrate day/night lighting transitions, UI Minimap, and basic sound effects (Andrii Lushchan, Xiangyu Zhang, Elif Tellal).

### Week 4 (Sunday, November 29, 2026 deadline) (Prototype - Subjected to changes) ###
- Full core-loop integration and playtesting (verifying the loop and shortcuts work end-to-end) (All Team Members).
- Fix critical blockers and prepare the playable build for the Prototype deadline (All Team Members).

### Milestone 1: Prototype Submission & Peer Evaluation (Sunday, December 6 – Wednesday, December 16, 2026) ###
- **December 6, 2026 @ 23:59:** Submit working Prototype build.
- **December 8 – December 16, 2026:** Prototype Peer Evaluation window; gather feedback from other students to log in the GDD.

### Milestone 2: Beta Submission & Peer Evaluation (Sunday, January 10 – Friday, February 19, 2027) ###
- Complete final pixel-art tilesets, NPC animations, full audio tracks, and balance adjustments based on December prototype feedback (All Team Members) **(Prototype - Subjected to changes)**.
- **January 10, 2027 @ 23:59:** Submit feature-complete Beta build.
- **January 11 – February 19, 2027:** Beta Peer Evaluation window.

### Milestone 3: Final Project Submission (Tuesday, February 23, 2027 @ 23:59) ###
- Final bug fixing, UI polish, and repository/GDD cleanup based on Beta evaluation feedback (All Team Members).
- **February 23, 2027 @ 23:59:** Submit Final Project build.
