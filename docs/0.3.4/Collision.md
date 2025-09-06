# Collision

?> `ICollidingComponent` is an interface for shapes that have collision with other classes that also uses this interface.
It has a function `CollideWith` to check if it is colliding to another component

### `CollidingSegment(v1, v2)`
---
 Returns: `CollidingSegment`

A line segment

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`v1` |`Vector2` |The first vertex of the line |
|`v2` |`Vector2` |The other vertex of the line |

### `CollidingSegment(center, length, rotation)`
---
 Returns: `CollidingSegment`

A line segment

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`center` |`Vector2` |The center of the line |
|`length` |`float` |The length of the line |
|`rotation` |`float` |The rotation of the line |

### `CollidingCircle(position, radius)`
---
 Returns: `CollidingCircle`

A circle with collision

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`position` |`Vector2` |The position of the circle |
|`radius` |`float` |The radius of the circle |

### `CollideRect(x, y, width, height)`
---
 Returns: `CollideRect`

A rectangle with collision

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`x` |`float` |The x coordinate of the top left corner of the rectangle |
|`y` |`float` |The y coordinate of the top left corner of the rectangle |
|`width` |`float` |The width of the rectangle |
|`height` |`float` |The height of the rectangle |

### `CollideRect(pos, size)`
---
 Returns: `CollideRect`

Creates a rectangle with collision with the given position and size

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`pos` |`Vector2` |The position of the rectangle |
|`size` |`Vector2` |The dimensions of the rectangle |

### `CollideRect(rect)`
---
 Returns: `CollideRect`

Creates a reatangle with collision from a `Rectangle`

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`rect` |`Rectangle` |The `Rectangle` to create from |
