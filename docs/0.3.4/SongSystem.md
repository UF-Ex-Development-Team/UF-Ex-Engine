# SongSystem

?> These are items that are related to the song system.
 
 There are two ways to set up a chart, `IWaveSet` and `IWaveSetS`, `IWaveSet` is the classic way to set up a chart, while the alternative is a more lightweight version of it.
 
 If you want the chart to be a championship chart (div.1, div.2, etc), you should also inherit `IChampionShip`
 
 Since these interfaces are rather straight forward, their functions will not be discussed here as it is already displayed in the example solution.
 
 However, one of the most important compoments is the `WaveConstructor`, it contains most of the useful functions you would use during charting.
 There are two awys to set up a `WaveConstructor`.

### `WaveConstructor(beatTime, [isBPM])`
---
 Returns: `void`

Initalizes the wave data

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`beatTime` |`float` |Duration of 1 beat or the BPM |
|`isBPM` |`bool` |Whether `beatTime` is the BPM itself (Default false) |



### `WaveConstructor(beats,...)`
---
 Returns: `void`

Initalizes the wave data

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`beats` |`float[][]` |Duration of each beat, [Beat Count, BPM] |


There are a lot of functions in `WaveConstructor` that are commonly used in charting

### `BeatTime(x)`
---
 Returns: `float`. Amount of frames for x beats

Duration of the given beat time in frames

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`x` |`float` |Amount of beats |

### `InBeat(beat)`
---
 Returns: `bool`. Whether the chart is currently at the given beat

Whether the chart is at the given beat

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`beat` |`float` |The beat to check |

### `InBeat(beat)`
---
 Returns: `bool`. Whether the chart is currently between the given beat

Whether the chart is currently in the range of the given beats

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`leftBeat` |`float` |Starting beat |
|`rightBeat` |`float` |Ending beat |

### `At0thBeat(beatCount)`
---
 Returns: `bool`. Whether the chart is at a multiple of the Xth beat

Check whether the chart is currently at a multiple of the given beat

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`beatCount` |`float` |The beat to check |

### `AtKthBeat(beatCount, K)`
---
 Returns: `bool`. Whether the chart is at a multiple of the Xth beat plus the frames given

Check whether the chart is currently at a multiple of the given beat plus the frames given

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`beatCount` |`float` |The beat to check |
|`K` |`float` |The frame remainder to check |

### `Delay(delay, action)`
---
 Returns: `void`

Invokes an action after the given frames

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delay` |`float` |The amount of frames to delay |
|`action` |`Action` |The action to invoke |



### `DelayBeat(delayBeat, action)`
---
 Returns: `void`

Invokes an action after the given beats

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delayBeat` |`float` |The amount of beats to delay |
|`action` |`Action` |The action to invoke |



### `ForBeat(durationBeat, action)`
---
 Returns: `void`

Invokes an action for the next given beats (Using int calculation, recommended not to use)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`durationBeat` |`float` |The duration of the action |
|`action` |`Action` |The action to invoke |



### `ForBeat(delayBeat, durationBeat, action)`
---
 Returns: `void`

Invokes an action for the next given beats after the given beats (Using int calculation, recommended not to use)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delayBeat` |`float` |The amount of beats to delay before invoking the action |
|`durationBeat` |`float` |The duration of the action |
|`action` |`Action` |The action to invoke |



### `ForBeat120(durationBeat, action)`
---
 Returns: `void`

Invokes an action for the next given beats (Using float calculation, recommended to use)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`durationBeat` |`float` |The duration of the action |
|`action` |`Action` |The action to invoke |



### `ForBeat120(delayBeat, durationBeat, action)`
---
 Returns: `void`

Invokes an action for the next given beats after the given beats (Using float calculation, recommended to use)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delayBeat` |`float` |The amount of beats to delay before invoking the action |
|`durationBeat` |`float` |The duration of the action |
|`action` |`Action` |The action to invoke |



### `ArrowAllocate(slot, direction)`
---
 Returns: `void`

Allocates a direction for arrows

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`slot` |`int` |The slot to allocate in (Range is [0, 9]) |
|`direction` |`int` |The direction to allocate |



### `RegisterFunction(name, action)`
---
 Returns: `void`

Registers a function for `CreateChart()` to execute

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`name` |`string` |The name of the function |
|`action` |`Action` |The action to invoke when executed |



### `RegisterFunctionOnce(name, action)`
---
 Returns: `void`

Registers a one time function for `CreateChart()` to execute

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`name` |`string` |The name of the function |
|`action` |`Action` |The action to invoke when executed |



### `CreateChart(delay, beat, arrowSpeed, barrage)`
---
 Returns: `void`

String based chart creator, use an empty string for an empty beat.

Optional Arguments: "!": No Score, "^": Accelerate, "&lt;": RotateL, "&gt;": RotateR, "*": Tap, "~": Void Sprite, "_": Hold

Order of parsing: ~*_&lt;&gt;^!

Direction Args: "R": Random, "D": Different, "+/-x" Add/Sub x to the last dir. , "$x": Fixed on x direction, "Nx": Not x, "Ax": The xth allocated direction

Optional Color Args: 0-> Blue, 1-> Red, 2-> Green, 3-> Purple

Optional Rotation Args: 0-> None, 1-> Reverse, 2-> Diagonal

GB：#xx#yz, Where "xx" means the duration beat, "y" beats direction, "z" means color, replace '#' by '%' if you don't want arrows

Combinations: "(R)(+0)", NOT "R(+0)"

Misc: use ' to multiply the speed of the arrow, &lt;&lt; or &gt;&gt; to adjust the current beat (>>0.5 will skip 0.5 beats)

Use `RegisterFunction()` or `RegisterFunctionOnce()` to declare functions to execute them inside here

For example RegisterFunctionOnce("func", ()=> {});

"(func)(R)", will invoke the action in "func" and creates an arrow

"!!X*/Y", the beats will be a 8 * X beat for the next Y beats (If Y is undefined then it will last for the rest of the function)

You can add arguments in the form of "&lt;Arg1,Arg2...&gt;Action"

You may use <see cref="Arguments"/> inside the declared action in RegisterFunction(Once) to access them.

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delay` |`float` |The delay for the events to be executed, generally used for preventing spawning immediately within view |
|`beat` |`float` |Duration of 8 beats, generally used with `BeatTime()` |
|`arrowSpeed` |`float` |The speed of the arrows |
|`barrage` |`string[]` |The array of strings that contains the barrage |



#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `Action<Arrow>` | ArrowProcesser | The process for all arrows that will be executed in `CreateChart()` |
 | `Arrow` | LastArrow | The last arrow created from `CreateChart()` |
 | `float` | CurrentTime | The current time calculated in `CreateChart()` |
 | `bool` | DelayEnabled | **UNKNOWN** |
 | `object[]` | Temps | Tempoary variable slot you can use, has a size of 100 |
 | `float[]` | Arguments | Arguments supplied to the function in the strings in `CreateChart()` |

 # Settings
 `WaveConstructor` has a subclass for some chart settings called `ChartSettings`, note that these variables only affect the events that are executedf in `CreateChart`.
 You can call them using `Settings.XXX`.
 #### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `float` | GBAppearVolume | The appearing volume of `GreenSoulGB` (Default 0.5f) |
 | `float` | GBShootVolume | The shooting volume of `GreenSoulGB` (Default 0.5f) |
 | `float` | VoidArrowVolume | The volume of collision of `Arrow` that has `VoidMode` set to true (Default 0.5f) |
 | `bool` | GreenTap | Whether all Tap arrows are displayed as green outlined arrows (Default false) |
 | `bool` | GBFollowing | Whether the `GreenSoulGB` follows the player rotation or not (Default false) |

 Besides these, you can add information to your chart using `SongInformation`
 #### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `bool` | MusicOptimized | Whether the music is an .ogg file |
 | `string` | SongAuthor | The composer of the song |
 | `string` | BarrageAuthor | The charter |
 | `string` | AttributeAuthor | The person who made the effects of the chart |
 | `string` | PaintAuthor | The artist of the chart cover |
 | `string` | DisplayName | The display name of the chart (Does not affect save data) |
 | `string` | Extra | Extra text displayed on the loading screen |
 | `Vector2` | ExtraPosition | The position of the `Extra` text |
 | `Color` | ExtraColor | The color of the `Extra` text |
 | `bool` | Hidden | Whether the chart is hidden or not, you can use a get; set; for setting this |
 | `Dictionary<Difficulty, float>` | CompleteDifficulty | The difficulty constants for completing the chart |
 | `Dictionary<Difficulty, float>` | ComplexDifficulty | The difficulty constant for acheving accuracy in the chart |
 | `Dictionary<Difficulty, float>` | APDifficulty | The difficulty constants for All Perfecting the chart |
 | `HashSet<Difficulty>` | UnlockedDifficulties | The difficulties that are unlocked, if they are locked, the player cannot play them |
 | `float[]` | MusicPreview | The beginning and end of the music preview (In seconds) |
 | `string[]` | Tags | The tags of the chart (Used in chart grouping) |
