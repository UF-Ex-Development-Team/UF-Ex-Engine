# Fight.Functions
`Fight.Functions` contains the most functions in the entire SDK, and are used on various occasions.
 
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `ContentManager` | Loader | The content loader, you can use this to load dynamic assets |
 | `Texture2D` | SongIllustration | The cover of the current chart (If any) |
 | `bool` | AutoEnd | Whether the chart automatically switch to the result screen after ending |
 | `float` | PlayOffset | When the song will begin playing (If the value is negative, `SongInformation.MusicOptimized` MUST be false) |
 | `float` | GametimeDelta | Gametime displacement of the chart (Initalization only) |
 | `int` | Gametime | Frames elapsed in integers (Readonly, Not recommended to use) |
 | `float` | GametimeF | Frames elapsed in float (Readonly, Recommended) |
 | `Difficulty` | CurrentDifficulty | The current difficulty of the chart |
 | `Player.Heart` | Heart | The soul you are currently controlling |
 | `Player` | PlayerInstance | The player you are currently controlling |
 
 ## ScreenDrawing
 This is a subclass of `Fight.Functions`, it is mostly for items that are related to the on screen items/effects

### `CustomSurface(surf, depth, blendState)`
---
 Returns: `void`

A class for creating custom surfaces

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`surf` |`Surface` |The surface to draw |
|`depth` |`float` |The depth of the surface |
|`blendState` |`BlendState` |The blend state of the surface (Default BlendState.AlphaBlend) |



### `CreateCustomSurface(surf, depth, blendState)`
---
 Returns: `void`

A method for creating custom surfaces

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`surf` |`Surface` |The surface to draw |
|`depth` |`float` |The depth of the surface |
|`blendState` |`BlendState` |The blend state of the surface (Default BlendState.AlphaBlend) |



### `SceneOut(col, time)`
---
 Returns: `void`

Fades out with the given color

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`col` |`Color` |The color to fade out |
|`time` |`float` |The duration of the fading |



### `WhiteOut(time)`
---
 Returns: `void`

Fades out in white

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The duration of the fading |



### `MakeFlicker(color)`
---
 Returns: `void`

Creates a flicker of the screen

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`color` |`Color` |The color of the flicker |



### `ActivateShader(shader, [depth])`
---
 Returns: `Shaders.Filter`

Activates a shader effect on the foreground

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`shader` |`Shader` |The shader to activate |
|`depth` |`float` |The depth of the shader |

### `ActivateShaderBack(shader, [depth])`
---
 Returns: `Shaders.Filter`

Activates a shader effect on the back ground

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`shader` |`Shader` |The shader to activate |
|`depth` |`float` |The depth of the shader |
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `Color` | UIColor | The color of the UI |
 | `Color` | BoundColor | The color of the side bounds |
 | `Color` | ThemeColor | The theme color of the chart |
 | `Color` | BoxBackColor | The background color of the box |
 | `Color` | BackGroundColor | The background color of the chart |
 | `float` | DownBoundDistance | The distance of the lower bound |
 | `float` | LeftBoundDistance | The distance of the leftward bound |
 | `float` | UpBoundDistance | The distance of the upper bound |
 | `float` | RightBoundDistance | The distance of the rightward bound |
 | `float` | ScreenAngle | The rotation of the screen |
 | `float` | ScreenScale | The scale of the screen |
 | `Vector2` | ScreenPositionDelta | The displacement of the screen |
 | `float` | SceneOutScale | The default fading speed of the color fading |
 | `float` | MasterAlpha | The overall Alpha value of the screen |
 | `RenderingManager` | SceneRendering | The main scene rendering manager |
 | `RenderingManager` | BackGroundRendering | The background scene rendering manager |
 | `SpriteBatchEX` | SpriteBatch | The sprite batch to draw sprites |

 There are several subclasses in `ScreenEffects` for even more effects.
 
 ## UISettings

### `CreateUISurface()`
---
 Returns: `UISurfaceDrawing`. You do not have to use the returned value

Creates a seperate surface for the UI, making the UI not affected by the screen effects

### `RemoveUISurface()`
---
 Returns: `void`

Removes the surface created in `CreateUISurface`
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `Vector2` | NameShowerPos | Position of the Name display (Default (20, 457)) |
 | `Vector2` | HPShowerPos | Position of the HP display (Default (320, 443)) |

 ## HPBar
#### Variable List
 | Type | Name | Description
 | ------ | ------ | ------ |
 | `bool` | Vertical | Whether the HP bar is vertical |
 | `Color` | HPExistColor | The color of existing HP of the HP bar |
 | `Color` | HPLoseColor | The color of existing HP of the HP bar |
 | `Color` | HPKRColor | The color of the KR bar |   

 ## CameraEffect

### `Rotate(rotation, time)`
---
 Returns: `void`

Rotate the camera by the given amount of degrees

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The angle to rotate for |
|`time` |`float` |The duration of the rotation |



### `RotateTo(rotation, time)`
---
 Returns: `void`

Rotates the screen angle to the given angle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rotation` |`float` |The angle to rotate to |
|`time` |`float` |The duration of the rotation |



### `Rotate180(time)`
---
 Returns: `void`

Rotates the screen by 180 degrees in the given time

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`float` |The duration of the rotation |



### `Convulse(direction)`
---
 Returns: `void`

Convulses the screen angle (16 degrees in 8 frames)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`direction` |`bool` |Direction of convulsion, true means right and false means left |



### `Convulse(time, direction)`
---
 Returns: `void`

Convulses the screen angle (25 degrees)

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`time` |`bool` |The duration of the convulsion |
|`direction` |`bool` |Direction of convulsion, true means right and false means left |



### `Convulse(intensity, time, direction)`
---
 Returns: `void`

Convulses the screen angle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`intensity` |`bool` |Intensity of the convulsion |
|`time` |`bool` |The duration of the convulsion |
|`direction` |`bool` |Direction of convulsion, true means right and false means left |



### `SizeExpand(intensity, time)`
---
 Returns: `void`

Expands the screen by the given size and then retracts to the original size

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`intensity` |`float` |The amount to expand |
|`time` |`float` |The duration of the expansion |



### `SizeShrink(intensity, time)`
---
 Returns: `void`

Retracts the screen by the given size and then expands to the original size

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`intensity` |`float` |The amount to retract |
|`time` |`float` |The duration of the retraction |



## HeartAttribute
 This is a subclass of `Fight.Functions`, it allows you to modify data of the soul (Some of them function the same as `Heart`).
 
## BoxStates
 This is a subclass of `Fight.Functions`, it allows you to modify data of the box.