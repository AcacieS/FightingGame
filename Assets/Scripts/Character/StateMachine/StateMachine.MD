# AI State Machine & Utility System

## Overview

This project uses a **hierarchical state machine combined with a utility-based decision system** to control AI characters.

The system is divided into two main parts:

1. **State Machine** — controls what the AI is currently doing.
2. **Utility System** — decides which action/state should be selected based on the current `Context`.

The general flow is:

```text
AIController
    │
    ▼
CompositeState
    │
    ├── UtilityAction → State 1
    ├── UtilityAction → Composite State -> ... State 2.1
    └── UtilityAction → State 3
              │
              ▼
          Execute State
              │
              ▼
       Request new decision
              │
              ▼
         AIController
              │
              ▼
       Start decision again
```

---

# 1. Main Components

The system consists of the following important classes:

| Class            | Purpose                                                      |
| ---------------- | ------------------------------------------------------------ |
| `AIController`   | Controls the overall state machine                           |
| `State`          | Base class for every AI state                                |
| `CompositeState` | Contains multiple states/actions and makes utility decisions |
| `UtilityAction`  | Connects considerations/requirements to a state              |
| `Consideration`  | Calculates how desirable an action is                        |
| `Requirement`    | Determines whether an action is allowed                      |
| `Utility`        | Selects an action using utility scores                       |
| `Context`        | Contains information about the current game situation        |
| `Character`      | Handles movement, health, facing direction, etc.             |

---
# General Implementation

### 1. Set up the `Context`

* Create a `Context` GameObject under your `Manager` GameObject.
* Add the `Context` script.
* Currently, assign:

  * `Self` → the character controlled by the AI.
  * `Target` → the character the AI is interacting with.
  * `AIController` → the AI controller associated with the character.
* **TODO:** In the future, the `GameManager` will handle these assignments automatically.

### 2. Set up the Character

* Add the `Character` script to the character's GameObject.
* Make sure the required components, such as `Rigidbody2D`, are also present.

### 3. Set up the AI State Machine

Create a child GameObject named `StateMachines` under the character and add the `AIController` script.

The `AIController` has a **root State**, which is usually a `CompositeState`.

#### State hierarchy example

```text
StateMachines
└── Root (CompositeState)
    ├── Offensive (CompositeState)
    │   ├── Attack (AttackState + UtilityAction + Considerations)
    │   └── Movement (MovementState + UtilityAction + Considerations)
    │
    └── Defensive (CompositeState)
        ├── Retreat (MovementState + UtilityAction + Considerations)
        └── Idle (IdleState + UtilityAction + Considerations)
```

### 4. Root `CompositeState`

* The root state should be a `CompositeState`.
* A `CompositeState` automatically collects its active child GameObjects containing a `State`.
* A `CompositeState` can contain other `CompositeState`s.
* This allows the state machine to be organized into multiple levels.

For example:

```text
Root
└── Offensive
    └── Attack
```

The root can choose `Offensive`, and then `Offensive` can make another decision to choose `Attack`.

### 5. Create a State

For each state GameObject, add:

#### Derived `State`

Create a class derived from `State` that contains the behavior you want.

Examples:

* `AttackState`
* `MovementState`
* `IdleState`
* `BlockState`

The state lifecycle is:

```text
Enter()
    ↓
Play()
    ↓
Exit()
```

* `Enter()` — called when the state starts.
* `Play()` — called every frame while the state is active.
* `Exit()` — called when leaving the state.

### 6. Add a `UtilityAction`

Add a `UtilityAction` component to the same GameObject as the State.

The `UtilityAction` determines **how desirable this state is**.

Usually, you don't need to modify the `UtilityAction` itself. It automatically collects its `Consideration`s and `Requirement`s when initialized.

```text
Attack
├── AttackState
├── UtilityAction
├── DistanceConsideration
└── HealthConsideration
└── DistanceRequirement
```

### 7. Add Considerations

Add one or more `Consideration`s to the State GameObject. \
naming Convention XConsideration: Consideration \
put in the consideration folder

For example:

```text
DistanceConsideration
```

A consideration evaluates the current `Context` and returns a score between **0 and 1**.

Each consideration has a **weight** that determines how strongly it contributes to the final score.

Example:

```text
DistanceConsideration
    Weight: 0.7

HealthConsideration
    Weight: 0.3
```

The `UtilityAction` combines these scores to determine how desirable the state is.

### 8. Add Requirements
naming Convention XRequirement: Requirement \
put in the requirement folder

Add a `Requirement` when a state should only be available under certain conditions.

A requirement acts as a **hard condition**.

For example:

```text
Attack
├── UtilityAction
├── DistanceConsideration
└── IsAliveRequirement
```

If a requirement is not met, the `UtilityAction` cannot be selected, regardless of its consideration score.

### 9. Decision Flow

The overall flow is:

```text
AIController
      ↓
Root CompositeState
      ↓
UtilityActions
      ↓
Requirements
      ↓
Considerations
      ↓
Choose State
      ↓
State.Enter()
      ↓
State.Play()
      ↓
Request Decision
      ↓
AIController makes a new decision
```

This allows the AI to continuously evaluate its situation and choose an appropriate behavior based on the current `Context`.


# 2. AIController

`AIController` is the top-level controller of the AI.

It is responsible for:

* Initializing the state hierarchy
* Starting the state machine
* Keeping track of the current state
* Starting new decisions
* Providing access to the `Character`
* Providing access to the `Context`

Conceptually:

```text
AIController
    │
    ├── Character
    ├── Context
    │
    └── Starting State
```

The `AIController` should be the only component responsible for starting the **root decision process**.

For example:

```csharp
public void RequestDecision()
{
    ChangeState(startingState);
}
```

The exact implementation can vary, but the important idea is that states should not manually rebuild the entire hierarchy.

---

# 3. State

Every AI behavior derives from `State`.

Example:

```csharp
public class IdleState : State
{
    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Idle");
    }

    public override void Play()
    {
        // Behavior while idle
    }

    public override void Exit()
    {
        // Cleanup
    }
}
```

A state has three main lifecycle methods:

```text
Enter()
    ↓
Play() which is Update()
    ↓
Exit()
```

### `Enter()`

Called once when the state starts.

Use it for:

* Resetting timers
* Saving starting positions
* Initializing variables
* Starting animations
* Facing the target

The base implementation currently makes the character face the target:

```csharp
public virtual void Enter()
{
    Context.Self.LookAt(Context.Target);
}
```

Therefore, child states should normally call:

```csharp
base.Enter();
```

---

### `Play()`

Called repeatedly while the state is active.

Use it for:

* Movement
* Timers
* Attacks
* Animations
* Checking whether the action is finished

Example:

```csharp
public override void Play()
{
    AI.Character.Move(1f);
}
```

---

### `Exit()`

Called when the state ends.

Use it for:

* Stopping movement
* Resetting temporary behavior
* Stopping animations
* Cleaning up

Example:

```csharp
public override void Exit()
{
    AI.Character.StopMoving();
}
```

---

# 4. CompositeState

A `CompositeState` is a state that contains other states.

Instead of directly deciding what to do, it uses `UtilityAction`s to choose its next child.

Example hierarchy:

```text
Root
└── Combat
    ├── Attack
    ├── Retreat
    └── Movement
```

`Combat` can be a `CompositeState`.

Its children can contain:

```text
Attack
    └── UtilityAction

Retreat
    └── UtilityAction

Movement
    └── UtilityAction
```

When `Combat` starts:

```text
Combat.Enter()
      ↓
MakeDecision()
      ↓
Utility chooses an action
      ↓
GetState()
      ↓
Child State.Enter()
```

---

# 5. Hierarchical State Machine

The system supports nested states.

For example:

```text
Root
│
└── Combat
    │
    ├── Offensive
    │   │
    │   ├── Attack
    │   └── Movement
    │
    └── Defensive
        │
        ├── Retreat
        └── Block
```

A `CompositeState` can contain another `CompositeState`.

This allows behavior to be organized hierarchically.

For example:

```text
Combat
    ↓
Offensive
    ↓
Movement
```

The AI can therefore make decisions at different levels.

---

# 6. UtilityAction

A `UtilityAction` connects:

* Requirements
* Considerations
* A State

Conceptually:

```text
UtilityAction
    │
    ├── Requirements
    │
    ├── Considerations
    │
    └── State
```

Example:

```text
Attack Action
    ├── Requirement: Is in attack range
    ├── Consideration: Target health
    ├── Consideration: Distance
    └── State: AttackState
```

The `UtilityAction` calculates a final score.

---

# 7. Requirements

A `Requirement` answers:

> **"Is this action allowed?"**

It is a hard condition.

Example:

```csharp
public class MaxDistanceRequirement : Requirement
{
    [SerializeField] private float maxDistance = 2f;

    public override bool IsMet(Context context)
    {
        return context.Distance <= maxDistance;
    }
}
```

If the requirement is not met:

```text
UtilityAction
      ↓
Requirement fails
      ↓
Action cannot execute
      ↓
Score = 0
```

Requirements should be used for things that **must** be true.

Examples:

```text
Target exists
Distance <= 2m
Character alive
Enough stamina
Target is visible
```

---

# 8. Considerations

A `Consideration` answers:

> **"How desirable is this action?"**

Unlike a requirement, it produces a value between:

```text
0 → 1
```

Example:

```csharp
public class DistanceConsideration : Consideration
{
    protected override float Calculate(Context context)
    {
        // Calculate desirability
    }
}
```

A consideration can also have a weight:

```csharp
[SerializeField, Range(0f, 1f)]
private float weight = 1f;
```

The final consideration value is multiplied by its weight.

Conceptually:

```text
Raw Score × Weight = Weighted Score
```

---

# 9. AnimationCurve Considerations

Considerations can use an `AnimationCurve` to define custom relationships.

For example:

```text
Weight
1.0 │●
    │ \
    │  \
0.5 │   ●
    │     \
    │       ●
0.0 └──────────── Distance
```

This allows designers to define behavior visually.

For example:

```text
Distance = 0m → Score 1.0
Distance = 5m → Score 0.5
Distance = 10m → Score 0.0
```

The curve is evaluated using a normalized value:

```csharp
float normalizedDistance =
    Mathf.InverseLerp(minDistance, maxDistance, distance);

return Mathf.Clamp01(
    curve.Evaluate(normalizedDistance)
);
```

---

# 10. Weight

Each consideration has a weight.

For example:

```text
Distance Consideration
Weight = 0.8

Health Consideration
Weight = 0.2
```

The utility action combines the considerations according to their weights.

This lets you control the importance of each factor.

A higher weight means:

> This consideration has more influence on the final decision.

---

# 11. UtilityAction Scoring

The general calculation is:

```text
For each consideration:

    consideration score
        ×
    consideration weight

Then combine the scores.
```

The current implementation effectively calculates a weighted average.

For example:

```text
Distance:
Score = 0.8
Weight = 0.7

Health:
Score = 0.4
Weight = 0.3
```

The final score is approximately:

```text
(0.8 × 0.7 + 0.4 × 0.3)
--------------------------------
             1.0

= 0.68
```

---

# 12. Choosing an Action

The utility system can be configured to choose an action based on its utility score.

For example:

```text
Attack       → 0.80
Approach     → 0.55
Retreat      → 0.25
Idle         → 0.10
```

A traditional utility AI would choose:

```text
Attack
```

However, this project can also use **weighted/random selection** if the goal is to avoid always choosing the highest-scoring action.

For example:

```text
Attack       → 80% influence
Approach     → 55% influence
Retreat      → 25% influence
```

This allows AI behavior to be less predictable.

---

# 13. Context

`Context` contains the information used by Requirements and Considerations.

Example data:

```text
Self
Target

Distance
Direction
DirectionSign

Self HP
Self Max HP

Target HP
Target Max HP

Target Is Attacking
Target Is Blocking
```

Instead of states directly calculating game information, they can use:

```csharp
Context.Distance
Context.Direction
Context.DirectionSign
Context.SelfHp
Context.TargetHp
```

This keeps decision-making centralized.

---

# 14. Context Direction

The direction is calculated from the AI toward the target:

```csharp
Direction =
    Target.transform.position.x -
    Self.transform.position.x;
```

Therefore:

```text
AI                         Target

  ● ------------------------>

Direction > 0
```

means the target is to the right.

And:

```text
Target                    AI

  ● ---------------------- ●

Direction < 0
```

means the target is to the left.

`DirectionSign` can then convert this to:

```text
-1 = left
+1 = right
```

This is useful for movement.

---

# 15. Character Movement

Movement is centralized inside `Character`.

Normal movement:

```csharp
AI.Character.Move(direction);
```

This uses:

```text
CharacterInfo.MoveSpeed
CharacterInfo.Acceleration
```

Custom movement:

```csharp
AI.Character.Move(
    direction,
    speed,
    acceleration
);
```

This allows individual states to override movement settings.

---

# 16. MovementState

`MovementState` can be configured to either:

* Move toward the target
* Move away from the target

Inspector:

```text
Movement Distance
Move Away From Target
Override Movement Settings
    Movement Speed
    Movement Acceleration
```

If:

```text
Move Away From Target = false
```

the character approaches.

If:

```text
Move Away From Target = true
```

the character retreats.

The state calculates:

```csharp
_movementDirection =
    moveAwayFromTarget
        ? -_direction
        : _direction;
```

---

# 17. Movement Distance

`MovementState` moves for a configured distance.

Example:

```text
Movement Distance = 3
```

The AI moves approximately 3 meters from where it started.

This is intentionally different from:

```text
"Move until you are 3 meters closer to the target."
```

The target can move during the action.

Instead, the state measures the AI's own movement:

```text
Start Position
      │
      │  3 meters
      ▼
Current Position
```

Once the distance has been reached:

```csharp
RequestRootDecision();
```

---

# 18. State Transitions

A state should not generally decide which state comes next directly.

Instead, it requests a new utility decision.

For example:

```csharp
if (distanceMoved >= _movementDistance)
{
    RequestRootDecision();
}
```

This sends control back to the `AIController`.

The decision process then starts again:

```text
Current State
     ↓
Action finished
     ↓
RequestRootDecision()
     ↓
AIController
     ↓
Root CompositeState
     ↓
Utility decision
     ↓
New State
```

This prevents deeply nested states from recursively calling each other indefinitely.

---

# 19. Why RequestRootDecision Exists

There are two possible decision requests:

```csharp
RequestDecision();
```

and:

```csharp
RequestRootDecision();
```

`RequestDecision()` asks the current parent `CompositeState` to make a decision.

This is useful for local hierarchical behavior.

`RequestRootDecision()` sends the decision request back to the `AIController`.

Use the root request when an action is completely finished and you want the **whole AI decision process** to start again.

For example:

```text
Combat
    ↓
Movement
    ↓
Movement finished
    ↓
AIController
    ↓
Choose completely new behavior
```

---

# 20. Facing the Target

The base `State.Enter()` currently does:

```csharp
Context.Self.LookAt(Context.Target);
```

This means every state automatically faces the target when it starts.

For example:

```csharp
public override void Enter()
{
    base.Enter();

    // State-specific setup
}
```

The character only changes direction once when entering the state.

It does **not** continuously turn toward the target during the state.

This is useful for actions such as attacks where the character should keep the direction it had when the action started.

---

# 21. Continuous Look At

`Character` can still provide a coroutine for situations where continuous tracking is desired.

For example:

```csharp
StartCoroutine(
    LookAtContinuously(target)
);
```

This is useful for behaviors that should continuously face another character.

However, normal AI states should generally use:

```csharp
LookAt(target);
```

once during `Enter()` rather than continuously changing direction.

---

# 22. Recommended Unity Hierarchy

A typical setup could look like:

```text
Enemy
├── Character
├── AIController
│
└── Root
    └── Combat
        ├── Attack
        │   └── UtilityAction
        │       ├── DistanceRequirement
        │       └── DistanceConsideration
        │
        ├── Movement
        │   └── UtilityAction
        │       └── DistanceConsideration
        │
        └── Retreat
            └── UtilityAction
                └── HealthConsideration
```

The exact hierarchy can be changed depending on the AI.

---

# 23. Creating a New State

To create a new behavior:

### Step 1 — Create the state

```csharp
public class AttackState : State
{
    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Attack");
    }

    public override void Play()
    {
        // Attack behavior
    }

    public override void Exit()
    {
        // Cleanup
    }
}
```

### Step 2 — Create a UtilityAction

Add a `UtilityAction` to the same GameObject as the state.

The action automatically finds the `State` on the same GameObject if the state field is empty.

### Step 3 — Add Requirements

Add Requirements if the action should only be available under certain conditions.

Example:

```text
MaxDistanceRequirement
```

### Step 4 — Add Considerations

Add Considerations to determine how desirable the action is.

Example:

```text
DistanceConsideration
HealthConsideration
```

### Step 5 — Add the action to the CompositeState

Use:

```text
SetActions / SetStates
```

depending on the setup.

---

# 24. Example: Attack

An attack might be configured as:

```text
AttackState
└── UtilityAction
    ├── MaxDistanceRequirement
    │     Max Distance = 2
    │
    └── DistanceConsideration
          Mode = Curve
```

The result:

```text
Target too far
    ↓
Requirement fails
    ↓
Cannot attack

Target close
    ↓
Requirement passes
    ↓
Consideration calculates score
    ↓
Attack becomes available
```

---

# 25. Example: Retreat

A retreat action might use:

```text
RetreatState
└── UtilityAction
    └── HealthConsideration
```

The lower the AI's health becomes, the higher the retreat score can become.

For example:

```text
Health       Retreat Score

100          0.0
75           0.2
50           0.5
25           0.8
10           1.0
```

This makes retreat more desirable when the AI is weak.

---

# 26. Example: Approach

An approach action can use distance:

```text
ApproachState
└── UtilityAction
    └── DistanceConsideration
```

For example:

```text
Far away
    ↓
High Approach score

Close
    ↓
Low Approach score
```

---

# 27. Inspector Workflow

For a new behavior, the general workflow is:

```text
Create State
     ↓
Create UtilityAction
     ↓
Assign State
     ↓
Add Requirements
     ↓
Add Considerations
     ↓
Configure weights
     ↓
Add to CompositeState
     ↓
Set starting/root state
     ↓
Play
```

The custom inspectors are intended to make this process easier by hiding fields that are not relevant to the current configuration.

---

# 28. Debugging

Useful debug information includes:

```csharp
Debug.Log($"Chosen action: {action.name}");
```

and:

```csharp
Debug.Log($"Entering state: {state.name}");
```

The `UtilityAction` also exposes its final score in the Inspector:

```text
Final Score
```

This makes it possible to see why an action was selected.

When debugging utility decisions, check these in order:

```text
1. Is the UtilityAction present?
2. Is its State assigned?
3. Are Requirements passing?
4. Are there Considerations?
5. Are their weights > 0?
6. What is the final score?
7. Is the CompositeState initialized?
8. Is the state actually being entered?
```

---

# 29. Common Problems

## "No UtilityAction was chosen!"

Check:

* The `CompositeState` has actions assigned.
* `SetActions` was run.
* The actions are active.
* Requirements are passing.
* At least one Consideration exists.
* Consideration weights are greater than zero.
* `UtilityAction.Initialize()` was called.

---

## State does not move

Check:

```text
Rigidbody2D exists
CharacterInfo exists
MoveSpeed > 0
Acceleration > 0
```

Also verify that movement is being performed during `Play()`.

---

## State keeps facing the target

Make sure `LookAt()` is not being called continuously inside `Play()`.

Prefer:

```csharp
public override void Enter()
{
    base.Enter();
}
```

rather than:

```csharp
public override void Play()
{
    AI.Character.LookAt(Context.Target);
}
```

---

## State transitions repeatedly

Avoid having nested states directly call each other's decisions recursively.

Prefer:

```text
State finishes
    ↓
RequestRootDecision()
    ↓
AIController
    ↓
New decision
```

rather than:

```text
State A
 ↓
State B
 ↓
State A
 ↓
State B
 ↓
...
```

---

# 30. Design Philosophy

The system separates **decision-making** from **behavior**.

### Context

Answers:

> What is happening?

### Requirement

Answers:

> Is this action allowed?

### Consideration

Answers:

> How desirable is this action?

### UtilityAction

Answers:

> How desirable is this complete action?

### CompositeState

Answers:

> Which behavior should I choose?

### State

Answers:

> What should the AI actually do?

### AIController

Answers:

> When should the AI make a new decision?

This separation makes the AI easier to extend without putting all decision-making into one large state class.

---

# 31. Adding More AI Behaviors

The system is designed so new behaviors can be added without changing the core architecture.

For example:

```text
Attack
Block
Dodge
Approach
Retreat
Patrol
Chase
Flee
Heal
Search
Idle
```

Each can be implemented as its own `State`.

Then utility decisions can be built using existing or new:

```text
Requirements
Considerations
```

For example:

```text
Dodge
├── TargetIsAttackingRequirement
├── DistanceConsideration
└── HealthConsideration
```

or:

```text
Heal
├── HasHealingItemRequirement
└── HealthConsideration
```

---

# 32. Summary

The complete system can be thought of as:

```text
                    ┌─────────────┐
                    │ AIController│
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │    State    │
                    │  Hierarchy   │
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │CompositeState│
                    └──────┬──────┘
                           │
                    Utility Decision
                           │
             ┌─────────────┼─────────────┐
             ▼             ▼             ▼
          Action A      Action B      Action C
             │             │             │
        Requirements  Requirements  Requirements
        Considerations Considerations Considerations
             │             │             │
             ▼             ▼             ▼
           State         State         State
             │             │             │
             └─────────────┼─────────────┘
                           │
                           ▼
                   RequestRootDecision()
                           │
                           ▼
                    AIController
                           │
                           └── Repeat
```

The key rule is:

> **States perform behavior. UtilityActions decide which behavior is desirable. Requirements determine whether behavior is possible. Considerations determine how desirable it is. Context provides the information needed to make those decisions. AIController controls the overall lifecycle.**
