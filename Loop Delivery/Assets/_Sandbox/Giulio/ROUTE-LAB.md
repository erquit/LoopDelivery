# District One Route Lab

Open `DistrictOneRouteLab.unity` and press Play. **WASD** or **arrows** move, **E** interacts, **Tab** opens the notebook, and **M** opens the map. **Enter** confirms the option shown in a dialogue.

Take the three parcels from Beppe. Fountain Square gives you a route that stays open. Market passage takes you toward the canal until 09:25, and the Printers’ Arcade opens with the code from the dispatch plate. The bridge crew exposes Parcel #0000’s address from 10:00 to 10:35. If you arrive within 35 in-game minutes of a counter or barge opening, you can choose to wait there. Waiting advances the town clock, so other appointments can pass.

Town Hall takes deliveries from 11:00 until 13:00. Return to Beppe for the shift stamp, and compare the witnessed address with Locker #0000. The four-minute day is a Route Lab tuning choice. The scene has its own notebook save.

## Working in parallel

| Work | Edit here |
| --- | --- |
| Delivery hours, gate and barge schedule, wait tolerance | `DistrictOneSchedule.asset` in the Inspector |
| Road geometry and minimap paths | `Scripts/DistrictOneLayout.cs` |
| Building placement, sign positions, and collision | `Editor/DistrictOneSceneBuilder.cs` |
| Decorative trees, shopfronts, lanterns, and planters | `Art/District One Street Dressing.prefab` in Prefab Mode |
| Timed street visuals | `Scripts/DistrictOneStreetEvents.cs` |
| Dispatch, delivery, clue and reset rules | `Scripts/DistrictOneSession.cs` |
| Manifest, map, prompts, and receipts | `Scripts/DistrictOneHud.cs` |

The scene is generated. Landmarks, roads, and colliders are rebuilt from code. To regenerate it, close Route Lab and use **Tools → Loop Delivery → Rebuild District One Route Lab**. The builder reuses the schedule asset and street dressing prefab, so Inspector and Prefab Mode edits carry through. Rebuild after changing the market closing time to update its sign. Assign one person to regenerate and commit the scene and Unity `.meta` files for each integration pass.

Additional sprites came from the supplied LimeZu Modern Exteriors packs. The native 16×16 pack supplies the kiosks, barrier, bench, streetlight, pot, and postbox. The RPG Maker MV animation sheets supply the pigeon and dock parcel frames. The selected PNGs are imported locally with point filtering.
