# Architecture — Don Quixote: The Knight of the Sorrowful Face

## Status
State machines fully designed. Class list draft complete. Ready for Unity implementation.

---

## Communication Pattern
Cross-machine communication via **C# Events/Delegates**.
- Clean, decoupled, idiomatic C#
- Direct method calls acceptable for intra-object communication (e.g. Quixote's two state machines talking to each other)
- ScriptableObject event channels deferred to full game if needed

---

## State Machines

### WindmillEntity
| State | Description |
|-------|-------------|
| `Idle` | Dormant, player not in range |
| `Windmill` | Mundane form, blades turning |
| `Dragon` | Delusion form, attackable for score |
| `Frozen` | Locked in mundane form during Quixote's Lucidity |

**Transitions:**
- `Idle → Windmill` — player enters proximity radius
- `Windmill ↔ Dragon` — random toggle, frequency accelerates over time
- `Windmill/Dragon → Frozen` — QuixotePerception enters Lucidity
- `Frozen → Windmill` — QuixotePerception Lucidity timer expires

---

### QuixoteController — Action State Machine
| State | Description |
|-------|-------------|
| `Idle` | Standing still, no input |
| `Walking` | On foot movement |
| `Riding` | Mounted on Rocinante, longer lance range |
| `Charging` | Mounted full-speed attack run |
| `Attacking` | Lance or sword strike animation |
| `Stunned` | Dazed, brief loss of control. Also triggers Lucidity |
| `KnockedDown` | On the ground, fully vulnerable. Also triggers Lucidity |
| `Recovering` | Rising, vulnerable window, reduced speed |

**Transitions:**
- `Idle → Walking` — directional input on foot
- `Idle → Riding` — mounts Rocinante
- `Riding → Charging` — sprint input while mounted
- `Charging → Attacking` — reaches target
- `Charging → KnockedDown` — Rocinante Stumbles mid-charge
- `Attacking → Stunned` — hits WindmillEntity in Windmill (mundane) state
- `Riding → KnockedDown` — Rocinante enters Knocked state
- `Stunned/KnockedDown → Recovering` — timer expires
- `Recovering → Idle` — recovery animation completes

---

### QuixoteController — Perception State Machine
| State | Description |
|-------|-------------|
| `Delusion` | Default. Sees dragons. Windmills transform. Scoring active |
| `Lucidity` | Spell broken. Windmills freeze in mundane form. Cannot score |

**Transitions:**
- `Delusion → Lucidity` — Sancho speaks OR Quixote enters Stunned/KnockedDown
- `Lucidity → Delusion` — lucidity timer expires

*These two Quixote machines run concurrently and independently, but interact.*

---

### SanchoPança — Action State Machine
| State | Description |
|-------|-------------|
| `Idle` | Standing still, waiting |
| `Walking` | Following Quixote on foot |
| `Riding` | Mounted on Rucio, following at a distance |
| `Running` | Hurrying to catch up or fleeing |
| `Cowering` | Ducking, hiding — classic Sancho during combat |
| `Helping` | Tending to fallen Quixote, speeds up recovery |

*No Attacking, Charging, or Stunned — Sancho avoids combat and absorbs nothing.*

**Transitions:**
- `Idle/Riding → Walking` — Quixote moves, Sancho follows
- `Walking → Running` — Quixote moves too far ahead, or nearby chaos
- `Walking/Running → Cowering` — WindmillEntity transforms to Dragon nearby (chance-based)
- `Cowering → Walking` — dragon transforms back or Lucidity kicks in
- `Any → Helping` — Quixote enters KnockedDown
- `Helping → Idle` — Quixote recovers

---

### SanchoPança — Behaviour State Machine
| State | Description |
|-------|-------------|
| `Idle` | Following Quixote silently |
| `Speaking` | Delivering reality comment — forces Quixote into Lucidity |
| `Worried` | Reacts to Quixote being KnockedDown. Visual/audio cue |

**Transitions:**
- `Idle → Speaking` — random timer fires (suppressed if Cowering or too far from Quixote)
- `Speaking → Idle` — speech duration expires
- `Idle → Worried` — Quixote enters KnockedDown
- `Worried → Idle` — Quixote recovers

*Sancho's two machines run concurrently: action (what he does physically) and behaviour (what he says/reacts).*

---

### Rocinante
| State | Description |
|-------|-------------|
| `Grazing` | Idle at distance, not engaged |
| `Trotting` | Following Quixote at riding pace |
| `Charging` | Full gallop toward target |
| `Spooked` | Rears up, briefly uncontrollable |
| `Stumbling` | Trips mid-charge — interrupts Quixote's Charging |
| `Exhausted` | Slowed, no charging available. Natural stamina system |
| `Knocked` | Thrown by windmill impact, Quixote dismounted |

**Transitions:**
- `Grazing → Trotting` — Quixote mounts
- `Trotting → Charging` — Quixote inputs sprint
- `Trotting → Spooked` — WindmillEntity transforms to Dragon nearby (chance-based)
- `Trotting → Exhausted` — stamina depletes
- `Charging → Stumbling` — random chance
- `Charging → Knocked` — windmill impact
- `Charging → Exhausted` — charge ends normally
- `Spooked → Trotting` — spook timer expires
- `Stumbling → Trotting` — stumble animation completes
- `Exhausted → Trotting` — rest timer expires
- `Knocked → Grazing` — recovery animation completes

---

### Rucio
| State | Description |
|-------|-------------|
| `Grazing` | Idle, unbothered, eating |
| `Plodding` | Slow steady follow — default, never hurries |
| `Stopping` | Decides to stop — Sancho gradually falls behind Quixote |
| `Braying` | Loud protest — triggered by nearby chaos or Dragon transformation |
| `Bolting` | Rare panic — only most extreme events trigger this |

**Transitions:**
- `Grazing → Plodding` — Sancho mounts
- `Plodding → Stopping` — random chance (stubborn donkey)
- `Stopping → Plodding` — Sancho coaxes him (timer or player input)
- `Plodding → Braying` — WindmillEntity transforms to Dragon nearby (lower chance than Rocinante Spook)
- `Braying → Plodding` — braying timer expires
- `Plodding → Bolting` — extreme event: Rocinante also Spooked simultaneously
- `Bolting → Plodding` — bolt timer expires

---

## Cross-Machine Interactions

| Trigger | Effect |
|---------|--------|
| Sancho behaviour enters `Speaking` | QuixotePerception `Delusion → Lucidity` |
| Sancho action enters `Cowering` | Suppresses Sancho behaviour `Speaking` trigger |
| Rucio `Stopping` | Sancho falls behind — reduces Speaking trigger chance (distance-based) |
| QuixotePerception enters `Lucidity` | WindmillEntity `Dragon/Windmill → Frozen` |
| QuixotePerception `Lucidity` timer expires | WindmillEntity `Frozen → Windmill`, QuixotePerception `→ Delusion` |
| QuixoteAction `Attacking` + WindmillEntity `Dragon` | Score hit, play hit FX |
| QuixoteAction `Attacking` + WindmillEntity `Windmill` | QuixoteAction `→ Stunned` + score penalty |
| QuixoteAction enters `Stunned` or `KnockedDown` | QuixotePerception `→ Lucidity` (physical shock = clarity) |
| QuixoteAction enters `KnockedDown` | Sancho action `→ Helping`, Sancho behaviour `→ Worried` |
| Sancho action `Helping` | Speeds up Quixote `Recovering` timer |
| WindmillEntity transforms to `Dragon` near Rocinante | Chance → Rocinante `→ Spooked` |
| WindmillEntity transforms to `Dragon` near Rucio | Lower chance → Rucio `→ Braying` |
| WindmillEntity transforms to `Dragon` near Sancho | Chance → Sancho action `→ Cowering` |
| Rocinante `Stumbling` during Charging | QuixoteAction `Charging → KnockedDown` |
| Rocinante enters `Knocked` | QuixoteAction `→ KnockedDown` (dismounted) |
| Rocinante `Spooked` + Rucio `Bolting` simultaneously | Comic chaos event — both riders dismounted |

---

## Class List (draft)

| Class | Responsibility |
|-------|---------------|
| `WindmillEntity` | Windmill state machine, visual swap, hitbox activation, proximity detection |
| `QuixoteController` | Player input, action state machine, perception state machine, attack logic |
| `SanchoPanza` | Action state machine, behaviour state machine, distance tracking, Helping effect |
| `Rocinante` | Horse state machine, stamina system, stumble RNG |
| `Rucio` | Donkey state machine, stopping RNG, proximity to Sancho |
| `GameManager` | Score, penalties, level state, win/lose conditions, chaos event triggers |
| `TransformationSystem` | Oscillation timing, acceleration curve, frequency management across WindmillEntities |
| `LuciditySystem` | Timer management for Lucidity duration, broadcast to WindmillEntities |

---

## Open Questions
- Number of WindmillEntity instances in the level?
- Does Rocinante perceive the Dragon (Spooked) or is it always mundane from his POV?
- Exact lucidity duration — long enough to feel like a reprieve, short enough to create urgency
- Sancho Speaking trigger: purely random, distance-weighted, or danger-weighted?
- Can the player influence Sancho or Rucio in any way?
- Rucio Stopping: purely comic/atmospheric or does the player have a way to coax him?
- Unity architecture pattern: MonoBehaviour-based or ScriptableObject-driven events?
- Animator Controller vs code-driven visual swaps for Windmill/Dragon?
