# GameStates

### `GameStates()` (*class*)

GameStates are used to get, set, and modify the state of the game

**Methods**
---
### `InstanceCreate(obj)` 
Returns: `void`

Creates an instance

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`obj` |`GameObject` |The object to create |


### `ResetTime()` 
Returns: `void`

Resets the Gametime back to 0

### `StartSong(params)` 
Returns: `void`

Starts a chart with the given SongFightingScene.SceneParams

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`params` |`SongFightingScene.SceneParams` |The paramters of the chart |


### `StartSong(wave, songIllustration, path, dif, judgeState, mode)` 
Returns: `void`

Starts a chart with the given parameters

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`wave` |`IWaveSet` |The chart wave |
|`songIllustration` |`Texture2D` |The illustration of the chart |
|`path` |`string` |The path to the music file |
|`dif` |`int` |The difficulty of the chart |
|`judgeState` |`JudgementState` |The judgement state of the chart |
|`mode` |`GameMode` |The gamemode of the chart |


### `ResetScene(scene)` 
Returns: `void`

Sets the current scene into a new one

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`scene` |`Scene` |The target scene to set to |


### `EndFight()` 
Returns: `void`

Ends the current fight/chart

### `Broadcast` 
Returns: `void`

Broadcast an event globally

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`gameEventArgs` |`GameEventArgs` |The event to broadcast |


### `DetectEvent` 
Returns: `List<GameEventArgs>`

Detect whether an event (Made from `Broadcast(GameEventArgs)`) has been called

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`ActionName` |`string` |The name of the event to detect |
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `SpriteBatchEX` | SpriteBatch | The sprite batch of the game|
 | `GraphicsDeviceManager` | GameWindow| The game window |
 | `Scene` | CurrentScene | The current scene of the game, i.e. `SongFightingScene` |
 | `int` | difficulty | The difficulty of the current chart in `int`, you can convert it back to `Difficulty` |
 | `bool` | ForceDisableTimeTips | Whether the time tips (Early, Late) are forcefully disabled |
