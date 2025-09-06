# IO.IOEvent

### `IOEvent()` (*class*)

**Methods**
---
### `WriteCustomFile(location, bytes)` 
Returns: `void`

Creates an custom encoded file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`location` |`string` |File path |


### `WriteTmpFile(location, bytes)` 
Returns: `void`

`WriteCustomFile()` but with a file format of '.tmpf', they function the exact same

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`location` |`string` |File path |


### `ReadCustomFile(path)` 
Returns: `List<byte>`

Reads the list of bytes of the custom image file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`path` |`string` |The list of bytes on the image file |

### `ReadTmpfFile(path)` 
Returns: `List<byte>`

Reads the list of bytes of the custom tmpf file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`path` |`string` |The list of bytes on the tmpf file |

### `StringToByte(strings)` 
Returns: `List<byte>`. The bytes of the string

Convert a string into bytes

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`strings` |`string` |The string to convert |

### `StringToByte(strings)` 
Returns: `List<byte>`. The bytes of the string

Convert a string into bytes

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`strings` |`List<string>` |The string to convert |

### `ByteToString(bytes)` 
Returns: `List<string>`. The strings of the bytes

Convert a bytes into strings

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`bytes` |`List<byte>` |The bytes to convert |
