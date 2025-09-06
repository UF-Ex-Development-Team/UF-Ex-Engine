# Extends
!> Most of the classes/functions/variables in `Extends` are considered to be obsolete due to their nature
 of being superseded or poor reliability, any undocumented items are considered to be so. They are only available for legacy support.
## Entities

### `Star(centre, scale)`
---
 Returns: `void`

Creates a star with static position

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The centre of the star |
|`scale` |`float` |The scale of the star |



### `Star(ease, scale)`
---
 Returns: `void`

Creates a star with an easing motion

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`ease` |`Func` |The easing motion of the star |
|`scale` |`float` |The scale of the star |


#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `int` | ColorType | The color type of the star |
 | `float` | rotatespeed | The rotation speed of the star |
 | `bool` | starshadow | Whether the star has a shadow |

### `Fireball(centre, scale)`
---
 Returns: `void`

Creates a fireball with static position

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`centre` |`Vector2` |The centre of the fireball |
|`scale` |`float` |The scale of the star |



### `Fireball(ease, scale)`
---
 Returns: `void`

Creates a fireball with an easing motion

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`ease` |`Func` |The easing motion of the fireball |
|`scale` |`float` |The scale of the fireball |


#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `int` | ColorType | The color type of the star |
 | `float` | Alpha | The alpha of the fireball |
 | `bool` | IsHidden | Whether the fireball is masked inside of the board |
## DrawingUtil
 !> Some functions in this class are obsolete as they are either
 > 1. Contains bugs
 > 2. Has a better alternative

### `SetScreenScale(size, duration)`
---
 Returns: `void`

Sets the screen scale to the target size in the given duration using Quadratic easing

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`size` |`float` |The target size of the screen |
|`duration` |`float` |The duration of the lerping |



### `ScreenAngle(angle, time)`
---
 Returns: `void`

Sets the screen angle to the target angle in the given duration using Quadratic easing

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`angle` |`float` |The target angle of the screen |
|`time` |`float` |The duration of the lerping |



### `Shock([interval], [range], [times])`
---
 Returns: `void`

Shakes the screen

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`internal` |`float` |The interval between each shake (Default 2 frames) |
|`range` |`float` |The shake intensity (Default 2 pixels) |
|`times` |`int` |Times to shake (Default 4 times) |



### `Shock(interval, rangeX, rangeY, times)`
---
 Returns: `void`

Shakes the screen

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`internal` |`float` |The interval between each shake |
|`rangeX` |`float` |The shake intensity of the x-coordinate |
|`rangeY` |`float` |The shake intensity of the y-coordinate |
|`times` |`int` |Times to shake |



### `FadeScreen(inDuration, duration, outDuration, [color])`
---
 Returns: `void`

Creates a screen fading in and out

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`inDuration` |`float` |The duration for the screen to fade in |
|`duration` |`float` |The duration of the fade screen |
|`outDuration` |`float` |The duration for the screen to fade out |
|`color` |`Color` |The color of the fading (Default black) |



### `RotateWithBack(duration, range)`
---
 Returns: `void`

Rotates the camera and rotates back to origin

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`duration` |`float` |The duration of the rotations |
|`range` |`float` |The magnitude of the rotation |



### `RotateSymmetricBack(duration, range)`
---
 Returns: `void`

Rotates the camera and rotates it to the negation of it before rotating it to the origin (10 -> 25 -> -25 -> 0)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`duration` |`float` |The duration of the rotations |
|`range` |`float` |The magnitude of the rotation |




