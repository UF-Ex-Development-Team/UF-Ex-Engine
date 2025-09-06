# Barrages

?> Barrages are attacks you see during charts, such as Bones and Spears

Barrages use an interface called `ICollideAble`, this is an interface for a player collidable instance. It contains a function `GetCollide(Heart)` to check collision with the player.
 
 It also inherits `Entity` and `ICustomMotion`, therefore you can access their variables and functions as well.
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `bool` | MarkScore | Whether the barrage count towards the score (Default true) |
 | `bool` | AutoDispose | Whether the barrage will automatically dispose itself when it goes offscreen after entering the screen |
 | `int` | ColorType | The color type of the barrage |
 | `Color[]` | ColorTypes | The colors for each green soul shield, 0-> Blue, 1 -> Red etc |
 | `bool` | Hidden | Whether the barrage will only be displayed inside the box |
 | `JudgementState` | JudgeState | The current `JudgementState` of the chart |

# Bones
 Here are the variables that are shared between all types of bones
 
 ## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | Length | The length of the bone |
 | `bool` | IsMasked | Whether the bone is masked inside of the box |
 | `float` | Alpha | The alpha of the bone |
 | `float` | ColorType | The color of the bone, 0-> White, 1-> Blue, 2-> Orange |
 | `void` | ResetColor(Color color) | Sets the drawing color of the bone |
 | `bool` | AutoDepth | Whether the depth will be automatically sorted by their color type(Will override the original depth) |

### `CentreCircleBone(startRotation, rotateSpeed, length, duration)`
---
 Returns: `CentreCircleBone`

Creates a rotating bone at the center of the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startRotation` |`float` |The initial angle of the bone |
|`rotateSpeed` |`float` |The rotation speed of the bone |
|`length` |`float` |The length of the bone |
|`duration` |`float` |The duration of the bone |
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | RotateSpeed | The rotation speed of the bone |
 | `float` | MissionLength | The target length of the bone |

### `SideCircleBone(startRotation, rotateSpeed, length, duration)`
---
 Returns: `SideCircleBone`

A rotating bone at the side of the box (The box must be a square)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startRotation` |`float` |The initial angle of the bone |
|`rotateSpeed` |`float` |The rotation speed of the bone |
|`length` |`float` |The length of the bone |
|`duration` |`float` |The duration of the bone |
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | RotateSpeed | The rotation speed of the bone |
 | `float` | MissionLength | The target length of the bone |

### `SwarmBone(length, roundTime, startTime, duration)`
---
 Returns: `SwarmBone`

A bone that cycles inside the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`length` |`float` |The length of the bone |
|`roundTime` |`float` |The duration of one cycle |
|`startTime` |`float` |The initial time in the cycle |
|`duration` |`float` |The duration of the bone |

### `CustomBone(positionRoute, rotationRoute, length)`
---
 Returns: `CustomBone`

Creates a custom bone with position easing, rotation easing, and fixed length

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`rotationRoute` |`EaseUnit` |The easing of the rotation of the bone |
|`length` |`float` |The length of the bone |

### `CustomBone(startPos, positionRoute, rotationRoute, length, duration)`
---
 Returns: `CustomBone`

Creates a custom bone with a custom position route, rotation easing, fixed length, and specified duration

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startPos` |`Vector2` |The initial position of the bone |
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`rotationRoute` |`EaseUnit` |The easing of the rotation of the bone |
|`length` |`float` |The length of the bone |
|`duration` |`float` |The duration of the bone |

### `CustomBone(startPos, positionRoute, rotationRoute, length, duration)`
---
 Returns: `CustomBone`

Creates a custom bone with a custom position route, fixed rotation, fixed length, and specified duration

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startPos` |`Vector2` |The initial position of the bone |
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`rotation` |`float` |The rotation of the bone |
|`length` |`float` |The length of the bone |
|`duration` |`float` |The duration of the bone |

### `CustomBone(startPos, positionRoute, rotationRoute, length)`
---
 Returns: `CustomBone`

Creates a custom bone with a custom position route, fixed rotation, and fixed length

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startPos` |`Vector2` |The initial position of the bone |
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`rotation` |`float` |The rotation of the bone |
|`length` |`float` |The length of the bone |

### `CustomBone(startPos, positionRoute, lengthRoute, rotationRoute)`
---
 Returns: `CustomBone`

Creates a custom bone with custom position route, custom length route, and custom position route

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startPos` |`Vector2` |The initial position of the bone |
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`lengthRoute` |`EaseUnit` |The easing of the length of the bone |
|`rotationRoute` |`EaseUnit` |The easing of the rotation of the bone |

### `CustomBone(startPos, positionRoute, rotationRoute, length)`
---
 Returns: `CustomBone`

Creates a custom bone with custom position route, custom position route, and a fixed length

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`startPos` |`Vector2` |The initial position of the bone |
|`positionRoute` |`EaseUnit` |The easing of the position of the bone |
|`rotationRoute` |`EaseUnit` |The easing of the rotation of the bone |
|`length` |`float` |The length of the bone |

 # SideBones
 The base class of a side bone, being `UpBone`, `DownBone`, `LeftBone`, and `RightBone`
 Here are the variables that are shared between all `SideBones`
 
 ## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | Speed | The speed of the bone |
 | `float` | MissionLength | The target length of the bone |
 | `float` | LengthLerpScale | The scale of the lerp animation of the length of the bone (Default 0.1f) |

### `DownBone(way, speed, length)`
---
 Returns: `DownBone`

Creates a bone at the bottom of the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the left or right side, true-> right, false-> left |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

### `DownBone(way, position, speed, length)`
---
 Returns: `DownBone`

Creates a bone at the bottom of the box with a specified x coordiante

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the left or right side, true-> right, false-> left |
|`position` |`float` |The initial x coordinate of the bone |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

### `TopBone(way, speed, length)`
---
 Returns: `TopBone`

Creates a bone at the top of the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the left or right side, true-> right, false-> left |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

### `TopBone(way, position, speed, length)`
---
 Returns: `TopBone`

Creates a bone at the top of the box with a specified x coordiante

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the left or right side, true-> right, false-> left |
|`position` |`float` |The initial x coordinate of the bone |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

### `LeftBone(way, speed, length)`
---
 Returns: `void`

Creates a bone at the left side of the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the upper or lower side, true-> lower, false-> upper |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |



### `LeftBone(way, position, speed, length)`
---
 Returns: `void`

Creates a bone at the left side of the box with a specified y coordinate

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the upper or lower side, true-> lower, false-> upper |
|`position` |`float` |The initial y coordinate of the bone |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |



### `RightBone(way, speed, length)`
---
 Returns: `RightBone`

Creates a bone at the right side of the box

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the upper or lower side, true-> lower, false-> upper |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

### `RightBone(way, position, speed, length)`
---
 Returns: `RightBone`

Creates a bone at the right side of the box with a specified y coordinate

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`way` |`bool` |Whether to spawn on the upper or lower side, true-> lower, false-> upper |
|`position` |`float` |The initial y coordinate of the bone |
|`speed` |`float` |The speed of the bone |
|`length` |`float` |The length of the bone |

 # Boneslab
 Or as some people call them, bone walls.

### `Boneslab(rotation, appearDelay, totalTime, lengthRoute, lengthRouteParam)`
---
 Returns: `Boneslab`

Creates a boneslab

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The rotation of the wall (Must be a multiple of 90) |
|`appearDelay` |`int` |The duration of the warning before spawning |
|`totalTime` |`int` |The duration of the boneslab |
|`lengthRoute` |`Func` |The route of the height of the boneslab |
|`lengthRouteParam` |`float[]` |The parameters of the route |

### `Boneslab(rotation, height, appearDelay, totalTime)`
---
 Returns: `Boneslab`

Creates a boneslab

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The rotation of the wall (Must be a multiple of 90) |
|`height` |`float` |The height of the boneslab |
|`appearDelay` |`float` |The duration of the warning before spawning |
|`totalTime` |`float` |The duration of the boneslab |

?> There is a `Action` variable `BoneProtruded` that executes an action when the boneslab is created (When the warning ends)

 # GasterBlaster
 There are two types of blasters, normal blasters and green soul blasters
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | AppearVolume | The volume of the blaster spawning (Default 0.85f) |
 | `float` | ShootVolume | The volume of the blaster shooting (Default 0.8f) |
 | `bool` | IsGBMute | Whether will the blaster have no sound played |
 | `bool` | IsShake | Whether the blaster shakes the screen when fired |
 | `bool` | ColorIsTheme | Whether the color of the blaster is the theme color |
 | `void` | Delay(float delay) | Delays the blaster by the given frames |
 | `void` | Stop(float delay) | Stops the blaster for the given frames |

### `GreenSoulGB(shootShieldTime, way, color, duration)`
---
 Returns: `GreenSoulGB`

Creates a green soul blaster

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`shootShieldTime` |`float` |The time for the blaster to fire |
|`way` |`string` |The string direction of the blaster |
|`color` |`int` |The color type of the blaster |
|`duration` |`float` |The duration of the blaster |

### `GreenSoulGB(shootShieldTime, way, color, duration)`
---
 Returns: `GreenSoulGB`

Creates a green soul blaster

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`shootShieldTime` |`float` |The time for the blaster to fire |
|`way` |`int` |The direction of the blaster |
|`color` |`int` |The color type of the blaster |
|`duration` |`float` |The duration of the blaster |
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `int` | Way | The direction of the blaster |
 | `Color` | DrawingColor | The drawing color type of the blaster |

### `NormalGB(missionPlace, spawnPlace, size, waitingTime, duration)`
---
 Returns: `NormalGB`

Creates a blaster that automatically aims towards the player

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`missionPlace` |`Vector2` |Target position |
|`spawnPlace` |`Vector2` |Initial position |
|`size` |`Vector2` |Size of the blaster(Width, Height), a small blaster is (1, 0.5f) and a big blaster is (1, 1) |
|`waitingTime` |`float` |Time required to pass before firing |
|`duration` |`float` |Duration of the blast |

### `NormalGB(missionPlace, spawnPlace, size, rotation, waitingTime, duration)`
---
 Returns: `NormalGB`

Creates a blaster that aims to the given angle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`missionPlace` |`Vector2` |Target position |
|`spawnPlace` |`Vector2` |Initial position |
|`size` |`Vector2` |Size of the blaster(Width, Height), a small blaster is (1, 0.5f) and a big blaster is (1, 1) |
|`rotation` |`float` |The target rotation of the blaster |
|`waitingTime` |`float` |Time required to pass before firing |
|`duration` |`float` |Duration of the blast |
# GunBullet
?> A `GunBullet` is a (Sudden Changes) bullet, do not confuse with a danmaku bullet(?).

### `GunBullet(targetCentre, delayTime, rotation)`
---
 Returns: `GunBullet`

Creates a (Sudden Changes) bullet

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`targetCentre` |`Vector2` |The position of the target |
|`delayTime` |`float` |The time delay of the bullet to fire |
|`rotation` |`float` |The angle of the bullet with respect to the target |

### `GunBullet(targetCentre, delayTime, rotations)`
---
 Returns: `GunBullet`

Creates multiple (Sudden Changes) bullets

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`targetCentre` |`Vector2` |The position of the target |
|`delayTime` |`float` |The time delay of the bullets to fire |
|`rotations` |`float[]` |The angle of the bullets with respect to the target |
# Spear
?> Do not confuse `Spear` and `Arrow`, `Spear` is used in red soul segments.
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `bool` | IsHidden | Whether the spear will be drawn exclusively inside the box or not |
 | `float` | Alpha | The alpha of the spear |
 | `Color` | DrawingColor | The drawing color of the spear |

### `NormalSpear(centre, [rotation], [speed])`
---
 Returns: `void`

Creates a normal spear

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The position to create the spear |
|`rotation` |`float` |The angle of the spear (Default aiming towards the player) |
|`speed` |`float` |The speed of the spear (Default 1.7f) |


## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | Speed | The speed of the spear (Default 1.7f) |
 | `float` | Acceleration | The acceleration of the spear (Default 0.12f) |
 | `bool` | IsMute | Whether the spear won't play the sound effects |
 | `bool` | DelayTargeting | Whether will it aim at the soul or not |
 | `float` | WaitingTime | The time to wait before launch (Default 59 frames, a little less than 1 second (62.5f)) |
 | `Vector2[]` | ReboundVertices | The list of vertices to bounce off from |
 | `bool` | Rebound | Whether the spear will bounce when reaching the `ReboundVertices` |
 | `int` | ReboundCount | The amount of times to bounce before stopping to bounce |
 | `float` | Duration | The duration of the spear (Default 200 frames) |

### `Pike(centre, rotation, waitingTime)`
---
 Returns: `void`

Creates a spear with alpha fade in animation

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The position to create the spear |
|`rotation` |`float` |The angle of the spear |
|`waitingTime` |`float` |The delay before the spear shoots |


## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | Speed | The speed of the spear (Default 9.7f) |
 | `float` | Acceleration | The acceleration of the spear (Default 0.41f) |
 | `bool` | IsSpawnMute | Whether the spawn sound is muted |
 | `bool` | IsShootMute | Whether the shooting sound is muted |

### `SwarmSpear(centre, rotation, waitingTime)`
---
 Returns: `void`

Creates a spear that aims towards a center

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotateCentre` |`Vector2` |The center of the circle |
|`linearSpeed` |`float` |The speed of the spear |
|`distance` |`float` |The initial distance between the spear and the target |
|`rotation` |`float` |The angle of the spear with respect to the center |
|`waitingTime` |`float` |The delay before the spear shoots |



### `CircleSpear(rotateCentre, rotateSpeed, linearSpeed, distance, [rotateFriction], [rotate_extra])`
---
 Returns: `void`

Creates a spear that moves around the target with a circular motion

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotateCentre` |`Vector2` |The center of the circle |
|`rotateSpeed` |`float` |The angular speed of the spear |
|`linearSpeed` |`float` |The speed of the spear after shooting |
|`distance` |`float` |The initial distance between the spear and the target |
|`rotation` |`float` |The angle of the spear with respect to the center |
|`rotateFriction` |`float` |The friction of the angular rotation (Default 0.01f) |
|`rotate_extra` |`float` |The extra angle of the spear (Default 0) |


# Arrow
 It is unadvised to manually create an arrow since `CreateChart` usually does it for you
## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `bool` | VoidMode | Whether the sprite of the arrow is a void arrow |
 | `float` | VolumeFactor | The volume of the arrow when blocked |
 | `bool` | NoScore | Whether the score of the arrow won't be marked |
 | `Player.Heart` | Mission | The target heart of the arrow |
 | `int` | ArrowColor | The color type of the arrow |
 | `int` | Way | The direction of the arrow (Right, Down, Left, Up) |
 | `float` | CentreRotationOffset | The rotation with respect to the target |
 | `float` | SelfRotationOffset | The rotation of the arrow itself |
 | `float` | Speed | The speed of the arrow |
 | `float` | Alpha | The alpha of the arrow |
 | `float` | AppearTime | The frames elapsed after creation |
 | `float` | BlockTime | The time when the arrow should be blocked |
 | `Vector2` | Offset | The position offset of the arrow |
 | `bool` | RotateOffset | Whether `CentreRotationOffset` is enabled |
 | `float` | LateWaitingScale | **UNKNOWN** |
 | `void` | ResetColor(int color) | Sets the color of the arrow |
 | `void` | Delay(float delay) | Delay the arrow by the given amount of time |
 | `void` | Stop(float delay) | Stops the arrow for the gievn amount of time |
 
 There are a few subclasses that are related to the easing animation of the arrows
 ##ArrowEasing
 This is the parent of the subclasses used for easing, there is only 1 function that you should take note of, that is `TagApply(string tagName)`.
  It applies the easing functions of a derived class to the arrow with the given tag.
 ```code
 //This is a derived class
 Arrow.EnsembleEasing easeX = new();
 //All arrows with the tag "X" will have the easing function defined in `easeX` applied
 easeX.TagApply("X");
 ```
 
 ### EnsembleEasing

### `DeltaEase(rotationEases,...)`
---
 Returns: `void`

Eases the angle of the arrow from its target

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotationEases` |`EaseUnit` |The easing function of the angle |



### `SelfRotationEase(rotationEases,...)`
---
 Returns: `void`

Eases the arrow's own angle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotationEases` |`EaseUnit` |The easing function of the angle |



### `DistanceEase(distanceEases,...)`
---
 Returns: `void`

Eases the arrow's distance

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`distanceEases` |`EaseUnit` |The easing function of the distance of the arrow |


### UnitEasing
 ## Variable List
 | Type | Name | Description 
 | ------ | ------ | ------ |
 | `float` | AppearTime | Time elapsed after spawning |
 | `float` | ApplyTime | The total time of the easing |
 | `float` | Distance | The distance between the arrow and the target |
 | `float` | SelfRotation | The rotation of the arrow itself |
 | `EaseUnit<Vector2>` | PositionEase | The easing of the position of the arrow |
 | `EaseUnit<float>` | RotationEase | The easing of the rotation of the arrow |
 | `EaseUnit<float>` | DistanceEase | The easing of the distance of the arrow |
 | `EaseUnit<float>` | AlphaEase | The easing of the alpha of the arrow |
### ClassicApplier

### `ApplyDelay(delay)`
---
 Returns: `void`

Applies an arrow delay effect

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`delay` |`float` |The amount to delay |



### `ApplyStop(stopTime)`
---
 Returns: `void`

Applies an arrow stop effect

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`stopTime` |`float` |The duration of time to stop |






