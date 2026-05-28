# Decisions Log — Don Quixote: The Knight of the Sorrowful Face

## Format
Each entry: **Decision** | **Rationale** | **Date**

---

## Decisions

### 1. Prototype genre: side-scrolling beat-em-up
More action-forward and visually immediate than narrative RPG. Better suited for investor demo impact. Narrative/dialogue vision preserved for full game.
*Date: 2026-05-20*

### 2. Prototype scope: single windmill level
Focused scope keeps the demo achievable and communicates the core mechanic (delusion/reality duality) clearly and quickly.
*Date: 2026-05-20*

### 3. Visual style: Brazilian Cordel / xilogravura woodcut aesthetic
Distinctive, culturally resonant, achievable without high-budget assets. Historical precedent of Cordel Don Quixote adaptations gives it legitimacy.
*Date: 2026-05-20*

### 4. Core mechanic: windmill/dragon transformation with hit penalty
Player must strike only in dragon state. Hitting windmill state is penalized. Single mechanic carries full thematic weight of the source material.
*Date: 2026-05-20*

### 5. Transformation trigger: proximity-based with random oscillation
Windmill awakens on player approach, then randomly toggles states with accelerating frequency. Natural, spatial, escalating difficulty.
*Date: 2026-05-20*

### 6. Sancho Pança: reality-disruptor NPC
Random comments temporarily suppress Quixote's delusion, freezing transformations. Introduces risk/reward pause dynamic. Role to be expanded later.
*Date: 2026-05-20*

### 7. Architecture: layered state machines
Windmill/Dragon state machine + Quixote Delusion/Lucidity state machine. Sancho drives transitions on Quixote's machine. Clean, scalable, mirrors the book's themes structurally.
*Date: 2026-05-20*

### 8. Version control: GitHub repository
Set up early, before Unity project is created. Keeps code and design docs in sync. Enables collaboration and rollback.
*Date: 2026-05-20*

### 9. Development tooling: Claude.ai Project + Claude Code
Design and architecture in Claude.ai Project. Active Unity development in Claude Code once project structure is stable.
*Date: 2026-05-20*

### 10. Sancho has dual state machines (action + behaviour)
Action machine handles physical states (Walking, Cowering, Helping etc). Behaviour machine handles reactions and speech (Speaking, Worried). Run concurrently, mirroring Quixote's dual machine pattern.
*Date: 2026-05-26*

### 11. Rucio added as separate entity
Sancho's donkey has his own state machine (Grazing, Plodding, Stopping, Braying, Bolting). Calm and stubborn counterpart to Rocinante. Stopping state creates distance-based suppression of Sancho's Speaking trigger.
*Date: 2026-05-26*

### 12. Cross-machine communication: C# Events/Delegates
Clean, decoupled, idiomatic C#. Direct method calls acceptable for intra-object communication (e.g. Quixote's two machines). ScriptableObject event channels deferred to full game.
*Date: 2026-05-26*

### 13. Animation: Unity 2D Animation package
Free, built-in, no extra plugins. Designer delivers separate body part PNGs. Rigging and animation done in Unity's Skinning Editor and Animation window. Spine deferred to full game.
*Date: 2026-05-26*

### 14. Character assets: separate part PNGs, not sprite sheets
Designer draws each character as separate body parts (head, torso, limbs etc) with transparency. No animation knowledge required from designer. Keeps designer focused on Cordel visual style.
*Date: 2026-05-26*

### 15. Level design: Unity Tilemap system
Built-in, no extra plugins. Designer delivers tile sprites as PNGs. Level built directly in Unity editor.
*Date: 2026-05-26*

### 16. Prototype animations kept minimal
Only essential animations implemented for investor demo. Some state machine states can share animations. Full animation polish deferred to full game.
*Date: 2026-05-26*
