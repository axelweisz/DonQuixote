# Core Mechanics — Don Quixote: The Knight of the Sorrowful Face

## Windmill/Dragon Transformation System
- **Trigger:** proximity-based — windmill awakens when Quixote enters a certain radius
- **Behavior:** randomly toggles between Windmill state and Dragon state
- **Escalation:** transformation frequency accelerates over time
- **Hit logic:**
  - Strike in Dragon state → damage dealt / points gained
  - Strike in Windmill state → penalty (point loss, self-damage, knockback — TBD)

## Sancho Pança — The Disruptor
- Randomly triggers "reality comments" during combat
- **Effect:** temporarily breaks Quixote's delusion — windmills freeze in Windmill state, dragons revert
- Creates a risk/reward pause moment — wait it out or keep swinging?
- Whether the player can influence Sancho's behavior → to be developed further

## Dual State Machine Architecture (emerging)
- The Windmill/Dragon has its own state machine (Windmill ↔ Dragon)
- Quixote has a parallel state machine (Delusion ↔ Lucidity)
- Sancho forces transitions on Quixote's state machine
- These two layered state machines are the core of the level's logic

## Open Questions
- Exact penalty for hitting windmill state (health loss? score penalty? knockback?)
- Sancho trigger: purely random or time-based or distance-based?
- Can the player silence or influence Sancho?
- Number of windmills in the level?
- Is there a boss windmill/dragon?
