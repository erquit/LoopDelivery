# Figma asset import

[Source artwork](https://www.figma.com/design/0gxSPHbLJtUVYIL281nIQU/Final?node-id=0-1)

The transparent sheets are imported at 16 pixels per unit with point filtering. Select a sheet in the Project window and expand it to find its named sprites.

| Sheet | Sprites |
| --- | --- |
| `Buildings_16x16.png` | Bakery, bookshop, Town Hall, flats, and the Depot room and facade artwork |
| `Props_16x16.png` | Trees, planters, lamps, benches, fountains, fences, and terrain |
| `Roads_16x16.png` | Roundabout and horizontal and vertical road examples |
| `Vehicles_16x16.png` | Native-resolution car and bus slices |
| `Courier_SpriteSheet.png` | Four-direction walking and idle frames, ride frames, and bicycle sprites |

The placeable art prefabs in `Assets/_Game/Prefabs/Art/FigmaImports` reference these slices. `Player_Figma_Courier` uses the new walking and idle frames through the existing animator override. The ride frames are available in the sheet.

Use the road examples as a guide when painting the playable network with the Tilemap. Keep the sheet and its `.meta` together in Git so the prefab and animation references resolve.
