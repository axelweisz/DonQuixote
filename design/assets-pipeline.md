# Assets Pipeline — Don Quixote: The Knight of the Sorrowful Face

## Status
Pipeline decided for prototype. Full game pipeline to be revisited when scope expands.

---

## Visual Style Requirements
**Brazilian Cordel / xilogravura woodcut aesthetic:**
- High contrast black and white with selective color
- Bold, rough-edged linework
- Flat and graphic — not painterly or detailed
- This style naturally suits part-based character assembly

---

## Character Assets — What the Designer Delivers

### Format
- **PNG with transparency** for every part
- Agree on a **pixels-per-unit standard** before production begins (e.g. 100px/unit)
- Consistent resolution across all characters so scaling is predictable in Unity

### Character Parts (per character)
Each character is drawn as **separate body part PNGs**, not as a single flat sprite. This enables skeletal animation in Unity without requiring the designer to know any animation software.

Typical part breakdown:
- Head
- Torso
- Upper arm (x2)
- Lower arm (x2)
- Hand (x2)
- Upper leg (x2)
- Lower leg (x2)
- Foot (x2)
- Any character-specific parts (e.g. Quixote's lance, helmet, Sancho's hat, Rocinante's head/body/legs separately)

### Designer Does NOT Need To:
- Know Spine, Dragon Bones, or any animation software
- Produce sprite sheets or animation frames
- Know anything about Unity

---

## Animation — Unity 2D Animation Package

### Tool
**Unity's built-in 2D Animation package** — free, no additional plugins required.

### Pipeline
1. Designer delivers separate part PNGs
2. Import into Unity as sprites
3. Use Unity's **Skinning Editor** to assemble parts and create skeleton rig
4. Animate using Unity's **Animation** window with the rig
5. Drive animations via **Animator Controller** connected to state machines

### Prototype Animation Priority
Keep animations minimal for the investor demo. Only implement what's essential to read clearly on screen:

| Character | Priority animations |
|-----------|-------------------|
| Quixote | Idle, Walking, Charging, Attacking, Stunned, KnockedDown |
| Sancho | Idle, Walking, Speaking (gesture), Cowering, Helping |
| Rocinante | Trotting, Charging, Stumbling, Spooked |
| Rucio | Plodding, Stopping, Braying |
| WindmillEntity | Windmill (blades turning), Dragon (idle threat), transition FX |

*Not every state machine state needs a unique animation for the prototype. Some states can share animations.*

---

## Level Design

### Tool
**Unity's built-in Tilemap system** — no extra plugins needed.

### Designer Delivers
- Tile sprites (ground, background elements, foreground details) as PNGs
- A style guide or reference sheet so tiles feel consistent

### Level is built directly in the Unity editor using the tiles provided.

---

## Environment / Windmill Assets
The windmill is the star of the level. It needs:
- **Windmill form:** blades, body, base — ideally with animated rotating blades
- **Dragon form:** full dragon sprite or assembled dragon parts for 2D Animation rig
- **Transition:** visual FX between forms (could be as simple as a flash/dissolve for the prototype)

---

## Full Game Pipeline (deferred)
For the full game, migrate to **Spine** (industry standard 2D skeletal animation):
- More powerful and flexible than Unity's 2D Animation package
- Designer can take more control of animation if they learn it
- Requires paid Spine license + Unity plugin
- Revisit when financing is secured

---

## Open Questions
- Pixels-per-unit standard to agree with designer (suggested: 100)
- Number of characters needing full rigs for prototype (Quixote, Sancho, Rocinante, Rucio, WindmillEntity = 5)
- Whether WindmillEntity dragon form is a full rig or a simpler sprite swap
- Background / environment art style — fully illustrated or tiled?
- Whether to use particle systems for hit FX or simple sprite flashes
