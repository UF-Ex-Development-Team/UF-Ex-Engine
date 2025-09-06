# Lines

?> Line is a subclass of `Entities`

### `Line` (*class*)

The class for a line effect

    These are the creation funcitons

### `Line(vec1, vec2)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`vec1` |`Vector2` |The position of the first end of the line |
|`vec2` |`Vector2` |The position of the second end of the line |

### `Line(centre, rotation)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The center of the line |
|`vec2` |`float` |The rotation of the line |

### `Line(Xcentre, rotation)` 
Returns: `Line`

Creates a line with the y coordinate being 240

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`Xcentre` |`float` |The x coordinate of the line |
|`vec2` |`float` |The rotation of the line |

### `Line(easing1, easing2)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`easing1` |`Func` |The easing of the first vertex |
|`easing2` |`Func` |The easing of the second vertex |

### `Line(centre, rotationEasing)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The center of the line |
|`rotationEasing` |`Func` |The easing of the rotation of the line |

### `Line(centreEasing, rotationEasing)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centreEasing` |`Func` |The easing of the center of the line |
|`rotationEasing` |`Func` |The easing of the rotation of the line |

### `Line(centreEasing, rotationEasing, lengthEasing)` 
Returns: `Line`

Creates a line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centreEasing` |`Func` |The easing of the center of the line |
|`rotationEasing` |`Func` |The easing of the rotation of the line |
|`lengthEasing` |`Func` |The easing of the length of the line from its center |

    These are functions you can call

### `AlphaDecrease(time, [val], [willDispose])` 
Returns: `void`

Fades out the line for the given duration

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The time taken for the line to fade out |
|`val` |`float` |The amount of alpha to decrease (Default entirely) |
|`willDispose` |`bool` |Whether the line will automatically dispose when the alpha reaches 0 (Default true) |

### `DelayAlphaDecrease(delay, time, [val])` 
Returns: `void`

Fades out the line by the given amount for the given duration after the given delay

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delay` |`float` |The delay before the line to fade |
|`time` |`float` |The time taken for the line to fade out |
|`val` |`float` |The amount of alpha to decrease (Default entirely) |

### `AlphaIncrease(time, [val])` 
Returns: `void`

Fades in the line by the given duration by the given value

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The time taken for the line to fade in |
|`val` |`float` |The amount to fade in (Default 1) |

### `DelayAlphaIncrease(delay, time, [val])` 
Returns: `void`

Fades in the line by the given amount for the given duration after the given delay

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delay` |`float` |The delay before the line to fade |
|`time` |`float` |The time taken for the line to fade out |
|`val` |`float` |The amount to fade in (Default 1) |

### `AlphaDecreaseAndIncrease(time, [val])` 
Returns: `void`

Decreases the alpha of the line and then increases it

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The time taken to complete the entire animation |
|`val` |`float` |The amount alpha to fade (Default 1) |

### `AlphaIncreaseAndDecrease(time, [val])` 
Returns: `void`

Increases the alpha of the line and then decreases it

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The time taken to complete the entire animation |
|`val` |`float` |The amount alpha to fade (Default 1) |

### `SplitLine(clear)` 
Returns: `Line`

Creates a clone of the line

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`clear` |`bool` |Whether to return the original line (true) or the splitted line (false) |

### `DelayDispose(v)` 
Returns: `void`

Disposes itself after the given amount of time

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`v` |`float` |The delay before disposing |

### `InsertRetention(effect)` 
Returns: `void`

Inserts a retention effect

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`effect` |`RetentionEffect` |The retention effect |

### `AddShadow(timeLag, alphaFactor)` 
Returns: `void`

Inserts a retention effect, the exact same as `InsertRetention(RetentionEffect)`

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`timeLag` |`float` |The delay before the effect spawns |
|`alphaFactor` |`float` |The alpha of the effect |

### `AddShadow(r)` 
Returns: `void`

Inserts a retention effect, the exact same as `InsertRetention(RetentionEffect)`

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`r` |`RetentionEffect` |The retention effect |
