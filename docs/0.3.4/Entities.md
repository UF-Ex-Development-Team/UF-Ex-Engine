# Entities

?> These are entites that are related to effects

### `ImageEntity(image)`
---
 Returns: `ImageEntity`

An entity that draws an image

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`image` |`Texture2D` |The image to draw |

This entity has a variable `OnDraw`, it overrides the drawing function if it is defined.

### `ParticleGather(centre, count, duration, color, [speed_range], [size_range])`
---
 Returns: `ParticleGather`

Creates particles that gather to the center

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The center position for the particles to gather to |
|`count` |`int` |The amount of particles to create |
|`duration` |`float` |The duration of the gathering |
|`color` |`Color` |The color of the particles |
|`speed_range` |`float[]` |The range of the speed of the particles (Default [2, 5]x of the image) |
|`size_range` |`float[]` |The range of the sizes of the particles (Default [0.4, 0.9]x of the image) |

### `Particle(color, speed, size, centre, [image])`
---
 Returns: `Particle`

Creates a particle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`color` |`Color` |The color of the particle |
|`speed` |`Vector2` |The speed of the particle |
|`size` |`float` |The size of the particle (In Pixels) |
|`centre` |`Vector2` |The position to create the particle |
|`image` |`Texture2D` |The image of the particle (Default `FightResources.Sprites.lightBall`) |

?> These are entites that are related to actions

### `InstantEvent(timeDelay, action)`
---
 Returns: `InstantEvent`

Invoke an action after the given delay

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`timeDelay` |`float` |The delay to invoke the action |
|`action` |`Action` | |

### `TimeRangedEvent(timeDelay, duration, action)`
---
 Returns: `TimeRangedEvent`

Invoke an action that lasts for the given duration after the given delay

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`timeDelay` |`float` |The delay to invoke the action |
|`duration` |`float` |The duration of the action to invoke |
|`action` |`Action` | |

### `TimeRangedEvent(duration, action)`
---
 Returns: `TimeRangedEvent`

Invoke an action that lasts for the given duration

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`duration` |`float` |The duration of the action to invoke |
|`action` |`Action` | |

?> These are entites that are related to the components on screen

### `AccuracyBar()`
---
 Returns: `AccuracyBar`

The accuracy bar at the bottom of the screen

 The `AccuracyBar` has a variable `EnabledGolden` to control whether the golden outline of arrows are enabled

### `NameShower()`
---
 Returns: `NameShower`

The name display entity
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `string` | level | The LV of the chart |
 | `float` | nameAlpha | The alpha of the name text |
 | `string` | OverrideName | The text to override the name with, set to `string.Empty` if to not override |

?> These are the entites that are related to the player

### `Player()`
---
 Returns: `Player`

The player entity
#### Variable/Function List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `Heart` | heartInstance | The heart instance of the player you are controlling |
 | `List<Heart>` | hearts | The list of hearts |
 | `HPControl `| hpControl | The HP controller entity |
 | ------ | ------ | ------ |
 | | HPControl Variables | |
 | `bool` | ScoreProtected | Whether score will not be counted during invincibility frames |
 | `void` | GiveProtectTime(int val, bool ProtectScore = false) | Applies the invincibility frames to the heart |

### `Heart()`
---
 Returns: `Heart`

The heart entity (Subclass of Player)
#### Variable/Function List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `FightBox` | controlingBox | The box the heart is tied to |
 | `int` | SoulType | Soul type, 0-> Red, 1-> Green, 2-> Blue, 3-> Orange, 4-> Purple, 5-> Gray |
 | `int` | ID | The ID of the player |
 | `Vector2` | LastCentre | The last position of the player |
 | `bool` | FixArrow | Whether the arrows will follow the rotation of the soul |
 | `float` | Alpha | The alpha of the soul |
 | `float` | Speed | The speed of the player (Default 2.5f) |
 | `int` | JumpTimeLimit | The maximum amount of times the player can jump (Default 2) |
 | `float` | JumpSpeed | The jumping speed of blue soul (Default 6) |
 | `float` | Gravity | The gravity of the blue soul (Default 9.8f) |
 | `bool` | UmbrellaAvailable | Whether the player can use the soft falling |
 | `bool` | SoftFalling | Whether to allow soft falling for blue soul |
 | `float` | UmbrellaSpeed | The speed of the slow falling of blue soul (Default 2/3f) |
 | `bool` | IsMoved | Whether the player is moving |
 | `bool` | IsStable | Whether the player is not moving |
 | `int` | PurpleLineCount | The amount of lines in purple soul mode |
 | `bool` | EnabledRedShield | Whether the enable the red shield for non-green soul types |
 | `bool` | IsSoulSplit | Whether the soul is split into several souls |
 | `bool` | IsOranged | Whether the soul is oranged (Forced to move constantly) |
 

?> The following will be the functions of `Heart`

### `Merge(another)`
---
 Returns: `void`

Merge the current soul with the target soul

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`another` |`Heart` |The soul to merge to |



### `MergeAll()`
---
 Returns: `void`

Merge all souls

### `Split()`
---
 Returns: `Heart`. The new soul that was split

Splits the current soul

### `InstantSplit(area)`
---
 Returns: `Heart`. The new soul that was split

Instantly splits the soul

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`area` |`CollideRect` |The rectangle of the box of the new soul |

### `ChangeColor(type, [resetGravSpd])`
---
 Returns: `void`

Changes the soul type

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`type` |`int` |Soul type, see `SoulType` for the types |
|`resetGravSpd` |`bool` |Whether to reset blue soul gravity or not (Default false) |



### `RotateTo(rot)`
---
 Returns: `void`

Rotates the soul to the given rotation

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rot` |`float` |The target angle |



### `InstantRotate(rot)`
---
 Returns: `void`

Instantly rotates the soul to the given rotation

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rot` |`float` |The target angle |



### `GiveForce(rotation, speed)`
---
 Returns: `void`

Applies force to the soul and changes the gravity of the soul to that direction

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The direction to set the gravity to (Must be a multiple of 90) |
|`speed` |`float` |The magnitude of gravity |



### `GiveInstantForce(rotation, speed)`
---
 Returns: `void`

Applies force to the soul and changes the gravity of the soul to that direction and instantly rotates the soul to that direction

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The direction to set the gravity to (Must be a multiple of 90) |
|`speed` |`float` |The magnitude of gravity |



### `Teleport(mission)`
---
 Returns: `void`

Lerps the player to the given position

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`mission` |`Vector2` |The target position |



### `InstantTP(mission)`
---
 Returns: `void`

Instantly teleports the player to the given position

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`mission` |`Vector2` |The target position |



### `FollowScreen(duration)`
---
 Returns: `void`

Sets the angle of the soul as the angle of the screen for the specified duration

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`duration` |`float` |The duration to set the angle of the soul for |



### `CreateCollideEffect2(color, size)`
---
 Returns: `void`

Creates a collision effect

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`color` |`Color` |The color of the effect |
|`size` |`float` |The size of the effect |



### `MoveState(col, moveFunction)`
---
 Returns: `MoveState`

The state of heart movement (Subclass of Player)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`col` |`Color` |The color of the heart |
|`moveFunction` |`Action` |The function of the heart movement |

?> These are the entites that are related to charting

### `Platform(platformType, startPos, positionRoute, rotation, length)`
---
 Returns: `Platform`

Creates a platform with fixed size

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`platformType` |`int` |The type of platform, 0-> Green, 1-> Purple |
|`startPos` |`Vector2` |The initial position of the platform |
|`positionRoute` |`Func` |The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of `startPos` and `positionRoute` |
|`rotation` |`float` |The rotation of the platform |
|`length` |`float` |The length of the platform |

### `Platform(platformType, startPos, positionRoute, rotation, length, duration)`
---
 Returns: `Platform`

Creates a platform with fixed size that lasts for a given duration before folding itself

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`platformType` |`int` |The type of platform, 0-> Green, 1-> Purple |
|`startPos` |`Vector2` |The initial position of the platform |
|`positionRoute` |`Func` |The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of `startPos` and `positionRoute` |
|`rotation` |`float` |The rotation of the platform |
|`length` |`float` |The length of the platform |
|`duration` |`float` |The duration of the platform |

### `Platform(platformType, startPos, positionRoute, lengthRoute, rotationRoute)`
---
 Returns: `Platform`

Creates a platform with fixed size that lasts for a given duration before folding itself

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`platformType` |`int` |The type of platform, 0-> Green, 1-> Purple |
|`startPos` |`Vector2` |The initial position of the platform |
|`positionRoute` |`Func` |The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of `startPos` and `positionRoute` |
|`lengthRoute` |`Func` |The easing of the size of the platform |
|`rotationRoute` |`Func` |The easing of the rotation of the platform |
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `float` | AppearTime | The time elapsed after the platform was created |
 | `bool` | isMasked | Whether the platform is masked inside the box |
 | `bool` | createWithScaling | Whether the platform will have a scaling animation when created |
 | `float` | scale | The default scale of the platform |
 | `void` | ChangeType() | Changes the type of the platform (Purple becomes Green and Green becomes Purple) |