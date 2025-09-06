# MathUtil

?> These are functions that are related to computation

### `InTriangle(v1, v2, v3, target)`
---
 Returns: `bool`. Whether the point is inside the triangle

Checks whether the point is inside of a triangle

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`v1` |`Vector2` |First vertex of the triangle |
|`v2` |`Vector2` |Second vertex of the triangle |
|`v3` |`Vector2` |Third vertex of the triangle |
|`target` |`Vector2` |The point to check |

### `PolygonCollide(polygon1, polygon2)`
---
 Returns: `bool`

Checks whether two polygons are colliding

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`polygon1` |`Vector2[]` |The list of vertices of the first polygon (In clockwise order) |
|`polygon2` |`Vector2[]` |The list of vertices of the second polygon (In clockwise order) |

### `FloatToString(val, [digits])`
---
 Returns: `string`. The string of the float

Converts a float to a string, regardless of decimal seperator

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`val` |`float` |The value to convert to string |
|`digits` |`int` |The rounding digit of the string |

### `FloatFromString(string)`
---
 Returns: `float`. The float from string

Converts a float from a string, regardless of decimal seperator

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`string` |`string` |The string to convert to a float |

### `RotationDist(rot1, rot2)`
---
 Returns: `float`. The angle difference, range is [0, 180]

Returns the minimal angle difference between two angles

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`val1` |`float` |The first angle |
|`val2` |`float` |The second angle |

### `Project(origin, vec)`
---
 Returns: `float`

Projects a vector onto the given vector

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`origin` |`Vector2` |The vector to project |
|`vec` |`Vector2` |The vector to project to |

### `Rotate(origin, rot)`
---
 Returns: `Vector2`. The rotated vector

Rotates the vector by the given angle in degrees

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`origin` |`Vector2` |The original vector |
|`rot` |`float` |The amount of degrees to rotate |

### `RotateRadian(origin, rad)`
---
 Returns: `Vector2`. The rotated vector

Rotates the vector by the given angle in radians

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`origin` |`Vector2` |The original vector |
|`rad` |`float` |The amount of degrees to rotate in radians |

### `MinRotate(rot1, rot2)`
---
 Returns: `float`. The angle difference, [0, 180]

Returns the minimal angle difference between two angles

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rot1` |`float` |The first angle |
|`rot2` |`float` |The second angle |

### `SignedPow(val, pow)`
---
 Returns: `float`. The number raised to the power `pow` maintaining the sign of `val`

Returns the value raised to the specific amount of power, maintaining it's original sign

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`val` |`float` |The value to raise |
|`pow` |`float` |The power to raise to |

### `Direction(start, end)`
---
 Returns: `float`. The angle between the two vectors

Get the angle in degrees between the two vectors

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`start` |`Vector2` |The starting vector |
|`end` |`Vector2` |The ending vector |

### `Direction(this vec)`
---
 Returns: `float`. The angle of the vector

Gets the direction of the vector with respect to the origin

### `Cross(this vec, vec2)`
---
 Returns: `float`. The determinant/cross product

The determinant/cross product of two vectors

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`vec2` |`Vector2` |The second vector |

### `Cross(this vec, vec2)`
---
 Returns: `float`. The cross product

The cross product of two 3D vectors

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`vec2` |`Vector2` |The second vector |

### `Clamp(min, val, max)`
---
 Returns: `int`. The clampped value

Clamps the value between the two specified values

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`min` |`int` |The minimum value |
|`val` |`int` |The value to set |
|`max` |`int` |The maximum value |

### `Clamp(min, val, max)`
---
 Returns: `float`. The clampped value

Clamps the value between the two specified values

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`min` |`float` |The minimum value |
|`val` |`float` |The value to set |
|`max` |`float` |The maximum value |

### `GetRadian(angle)`
---
 Returns: `float`. The angle in radians

Gets the radian equivalent of the angle in degrees

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`angle` |`float` |The angle to convert to radians |

### `GetAngle(angle)`
---
 Returns: `float`. The angle in degrees

Gets the degree equivalent of the angle in radians

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`angle` |`float` |The angle to convert to degrees |

### `GetVector2(length, angle)`
---
 Returns: `Vector2`

Calculates the `Vector2` using the given `length` and `angle`.

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`length` |`float` |The specified length of the vector |
|`angle` |`float` |The specified angle (in degrees) |

### `GetDistance(p1, p2)`
---
 Returns: `float`

Gets the distance between two vectors

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`p1` |`Vector2` |The first vector |
|`p2` |`Vector2` |The second vector |

### `GetRandom(x, y)`
---
 Returns: `int`. The random value

Gets a random value at [x, y]

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`x` |`int` |The minimum value |
|`y` |`int` |The maximum value |

### `GetRandom(x, y)`
---
 Returns: `float`. The random value

Gets a random value at [x, y]

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`x` |`float` |The minimum value |
|`y` |`float` |The maximum value |

### `Sqrt(v)`
---
 Returns: `float`. The square root of `v`

Returns the square root of the specified number

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`v` |`float` |The value to get the square root of |

### `RandIn<T>(inputs)`
---
 Returns: `T`. The random value from the specified values

Gets a random value from the specified values

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`inputs` |`T[]` |The values for getting |

### `Cos(a, b)`
---
 Returns: `float`. The cosine value of the angle between the two vectors

The cosine value from vector calculation

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`a` |`Vector2` |The first vector |
|`b` |`Vector2` |The second vector |

### `StringHash(string)`
---
 Returns: `uint`. The hashed value

Hashes the given string

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`string` |`string` |The string to hash |

### `GetHashCode(string)`
---
 Returns: `ulong`. The hash code

Gets the hash code of the string

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`string` |`string` |The string to hash |

### `QuickPow(a, b)`
---
 Returns: `ulong`. The value raised to the given power

Raises `a` by the power of `b`

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`a` |`ulong` |The value to raise |
|`b` |`ulong` |The power to raise to |

### `Posmod(a, b)`
---
 Returns: `int`. The wrapped value

Value wrap-around of `a` between [0, `b`]

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`a` |`int` |The value to wrap around |
|`b` |`int` |The max value that can be attained by `a` |

### `Posmod(a, b)`
---
 Returns: `float`. The wrapped value

Value wrap-around of `a` between [0, `b`]

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`a` |`float` |The value to wrap around |
|`b` |`float` |The max value that can be attained by `a` |

### `Encrypt(password, key)`
---
 Returns: `string`. The enrypted string

RSA encryption for a string

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`password` |`string` |The string to encrypt |
|`key` |`string` |The key of encryption |

### `Decrypt(password, key)`
---
 Returns: `string`. The decrypted string

RSA decryption for a string

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`password` |`string` |The string to decrypt |
|`key` |`string` |The key of decryption |

### `InRange<T>(this value, min, max)`
---
 Returns: `bool`. Whether the value is within the range

Check whether the value is inside of a range

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`value` |`IComparable` |The value to check |
|`min` |`IComparable` |The minimum range |
|`max` |`IComparable` |The maximum range |
