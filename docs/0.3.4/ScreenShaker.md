# ScreenShaker

?> ScreenShaker is a subclass of `Entities.Advanced`

### `ScreenShaker(shakeCount, shakeIntensity, shakeDelay, [startAngle], [angleDelta], [shakeFriction])`
---
 Returns: `ScreenShaker`

An entity that shakes the screen

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`shakeCount` |`int` |The amount of times to shake |
|`shakeIntensity` |`float` |The intensity (in pixels) of the shaking |
|`shakeDelay` |`float` |The delay between each shake |
|`startAngle` |`float` |The initial angle of the shaking (Default random) |
|`angleDelta` |`float` |The angle difference between each shake (Default randomized between 120 and 240) |
|`shakeFriction` |`float` |The percentage decrease of the intensity of each shake (Default 85%) |
