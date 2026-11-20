# Art sources and direction

The approved attic is the visual authority. Do not replace, repaint, quantize or resize its source PNG when regenerating game content. Unity imports it with Point filtering, no mipmaps and a 26-unit world width. The ghost occupies approximately one fifth of the large dresser's height and about one tenth of the game viewport height.

## Original character

Preserved source: `Assets/Game/Art/Ghost/Source/ghost_source.png`.
Editor processing creates separate transparent and blink copies. Flood fill removes only border-connected checkerboard colors; opaque white fabric and black facial features remain.

## Background files

All final PNGs are under `Assets/Game/Art/Backgrounds/Source/`. All were generated using the built-in imagegen tool, then copied unchanged into the project. The first was explicitly approved by the user; subsequent areas reference it.

| Area | Final file | Generation output ID |
| --- | --- | --- |
| Attic | attic-v2.png | exec-6755c72d-4ad9-4143-b2e6-173c5d880dca |
| Child room | childroom-v2.png | exec-aabdbd3f-22eb-4453-a0db-c1608c6e3667 |
| Kitchen | kitchen-v2.png | exec-619274d6-4c97-477f-9b82-2ffc8d0cd07a |
| Workshop | workshop-v2.png | exec-87b0b239-d2bf-4fd5-b8c1-4952aead30ca |
| Clock hall | clockhall-v2.png | exec-d86ac0cc-4ffc-4b77-85b8-4bd19b7f084f |


## Prompt direction

The following records the creative constraints rather than a verbatim tool transcript: a side-on 2D pixel-art room for a tiny white ghost, monochrome grey palette, visible square pixel clusters, detailed worn wooden furniture, cozy moonlit atmosphere, no perspective camera, no characters, no UI and no lettering. Furniture must establish the small character scale; the lower route must remain readable.

- Attic: heavy rafters, wooden wall, worn dresser and books, moonlit window, paper box and floorboards.
- Child room: preserve the approved attic's pixel density, palette and detail; old toys and children's furniture, quiet moonlight.
- Kitchen: the same side-on style, old kitchen fittings, utensils and cupboards, a quiet abandoned domestic space.
- Workshop: the same style, wooden workbench, clocks, mechanical details and tools.
- Clock hall: the same style, oversized old clock, wooden architectural details, open space for the final route and morning transition.

Foreground objects, platforms, particles and procedural sound are authored by `PixelArtGenerator` and `ShorokContentBuilder`. Superseded rough background PNGs are unreferenced and are not included in the release. There are no third-party stock background assets. Font license: `Assets/Game/Art/UI/OFL.txt`.
