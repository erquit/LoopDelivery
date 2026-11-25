# Figma asset import

Source: https://www.figma.com/design/0gxSPHbLJtUVYIL281nIQU/Final?node-id=0-1

The PNGs are exported from named Figma layers and imported as Unity sprites.

## Use in the game

- Buildings and individual props have matching prefabs in `Assets/_Game/Prefabs/Art/FigmaImports`.
- Road strips and the roundabout are examples; build the final road network with Unity Tilemap so the road tiles remain reusable.
- `CourierRide_SpriteSheet.png` has named directional slices and eight idle/walk clips. `Player_Figma_Courier.prefab` reuses the project player input and movement setup with those animations. The two bicycle frames are included; the current controller does not yet switch to bicycle-specific artwork.
- Vehicle sprite sheets remain available for slicing in Unity Sprite Editor before placing individual cars.
- Point filtering is enabled for pixel art. The 16x16 families use 16 pixels per unit; vehicle sheets use 32 pixels per unit.

## Imported nodes

| File | Figma node | Category |
|---|---:|---|
| `ShoppingCenter_3.png` | `11:2` | Commercial building |
| `ShoppingCenter_2.png` | `11:3` | Commercial building |
| `CityProps_337.png` | `11:4` | City prop |
| `CityProps_336.png` | `11:5` | City prop |
| `CityProps_334.png` | `11:6` | City prop |
| `CityProps_335.png` | `11:7` | City prop |
| `CityProps_333.png` | `11:8` | City prop |
| `CityProps_332.png` | `11:9` | City prop |
| `CityProps_331.png` | `11:10` | City prop |
| `TerrainFence_1.png` | `11:11` | Terrain and fence |
| `GenericBuilding_4.png` | `11:12` | Building |
| `GenericBuilding_3.png` | `11:13` | Building |
| `GenericBuilding_2.png` | `11:14` | Building / town hall |
| `CourierRide_SpriteSheet.png` | `11:15` | Courier animation sprite sheet |
| `CourierDepot.png` | `11:4002` | Courier depot |
| `CityProps_330.png` | `11:4003` | City prop |
| `Roundabout_Example.png` | `11:4004` | Road layout example |
| `RoadExtension_South.png` | `11:4005` | Road and sidewalk strip |
| `RoadExtension_West.png` | `11:4010` | Road and sidewalk strip |
| `RoadExtension_East.png` | `11:4015` | Road and sidewalk strip |
| `RoadExtension_North.png` | `11:4020` | Road and sidewalk strip |
| `StreetLamp_1.png` | `11:4025` | Street lamp |
| `StreetLamp_2.png` | `11:4026` | Street lamp |
| `CityProps_338.png` | `11:4027` | City prop |
| `Vehicles_1.png` | `11:4028` | Vehicle sprite sheet |
| `Vehicles_2.png` | `11:4029` | Vehicle sprite sheet |
| `Tileset_55.png` | `11:4030` | Tileset crop |

Each prefab uses its matching sprite in a `SpriteRenderer`. Add colliders and interaction components when the level design needs them.
