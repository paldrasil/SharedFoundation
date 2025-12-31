# Shared Foundation

Shared foundation library for Paldrasil games, providing core systems and utilities.

## Features

### Store System (Flux Pattern)
State management system following the Flux pattern with middleware and reducers:
- **DataStore**: Centralized state management with middleware chain
- **BaseReducer**: Base class for reducers that handle actions
- **StateCollection**: Manages collection of states
- **FluxAction**: Action object for dispatching

### Utility Classes

#### Event System
- **EventBus**: Type-safe event bus system with subscribe/unsubscribe
  - Supports generic events
  - Auto-dispose subscriptions
  - Thread-safe operations

#### Service Locator
- **Locator<T>**: Service locator pattern with lock mechanism
  - Singleton pattern for services
  - Lock to prevent override after setting
  - Type-safe service resolution

#### Object Pooling
- **ObjectPool**: Basic object pooling
- **ObjectsPool**: Advanced object pooling with prefab management

#### Utilities
- **EventBus**: Event system
- **Rng**: Random number generator with seed support
- **SeedUtil**: Utilities for seed generation
- **ConfigLoader**: Load configuration files
- **ChecksumUtil**: Checksum utilities
- **CRC32**: CRC32 implementation
- **CSVSheet**: CSV parsing utilities
- **SimpleJSON**: JSON parsing
- **WebRequestQueue**: Queue for web requests
- **ComponentExtensions**: Extension methods for Unity components
- **StringExtensions**: Extension methods for strings
- **UIListUtils**: Utilities for UI lists
- **LineUtil**: Line/geometry utilities
- **PolygonUtil**: Polygon utilities
- **OrbitCamera**: Orbit camera controller
- **Job**: Job system utilities
- **BTSimple**: Simple behavior tree utilities

### Editor Tools
- **ButtonDrawer**: Custom drawer for button attributes
- **DatabaseSOSyncEditor**: Editor tool to sync ScriptableObject databases

## Installation

This package is automatically installed in Unity projects. No external dependencies required.

## Usage

### DataStore

```csharp
using Shared.Foundation;

// Initialize store
var stateCollection = new StateCollection();
var reducers = new BaseReducer[] { /* your reducers */ };
var middlewares = new DataStore.MiddlewareCall[] { /* your middlewares */ };

DataStore.instance.Initialize(stateCollection, middlewares, reducers);

// Dispatch action
var action = new FluxAction { type = "ACTION_TYPE", payload = data };
DataStore.instance.Dispatch(action, (a) => {
    Debug.Log("Action completed");
});

// Get state
var state = DataStore.instance.GetState("stateName");
```

### EventBus

```csharp
using Shared.Foundation;

var eventBus = new EventBus();

// Subscribe
var subscription = eventBus.Subscribe<MyEvent>(e => {
    Debug.Log($"Event received: {e.data}");
});

// Publish
eventBus.Publish(new MyEvent { data = "Hello" });

// Unsubscribe (auto-dispose)
subscription.Dispose();
```

### Locator

```csharp
using Shared.Foundation;

// Set service
Locator<IMyService>.Set(new MyService(), lockOnce: true);

// Get service
if (Locator<IMyService>.TryGet(out var service))
{
    service.DoSomething();
}
```

### BTSimple (Behavior Tree)

Simple behavior tree implementation for reactive AI systems:

- **BTNode**: Base abstract class for all nodes
- **Selector**: Reactive priority selector that auto-resets old running branch when switching
- **Sequence**: Memory sequence that keeps index while Running
- **Condition**: Leaf node that evaluates a predicate function
- **ActionNode**: Leaf node that executes an action function

```csharp
using Shared.Foundation;

// Create a behavior tree
var root = new Selector("Root")
    .Add(new Sequence("Patrol Sequence")
        .Add(new Condition("Is Idle", () => enemyState == EnemyState.Idle))
        .Add(new ActionNode("Start Patrol", () => {
            StartPatrol();
            return NodeState.Running;
        })))
    .Add(new Sequence("Attack Sequence")
        .Add(new Condition("Can See Player", () => CanSeePlayer()))
        .Add(new ActionNode("Attack", () => {
            Attack();
            return NodeState.Success;
        })));

// Tick the tree (typically in Update)
var state = root.Tick();

// Reset the tree if needed
root.Reset();
```

**Key Features:**
- **Reactive Selector**: Automatically resets the previously running branch when switching to a new priority branch
- **Memory Sequence**: Remembers progress through children while Running, resets on Failure
- **Named Nodes**: All nodes support custom names for debugging
- **State Tracking**: Each node tracks LastState and LastTickFrame

## Requirements

- Unity 6000.2 or higher

## License

Internal package for Paldrasil games.
