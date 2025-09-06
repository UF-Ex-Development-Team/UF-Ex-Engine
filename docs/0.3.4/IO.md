# IO

## `IOEvent()` (*class*)

**Methods**
---
### `.WriteCustomFile(location, bytes)` 
Returns: `void`

Creates an custom encoded file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`location` |`string` |File path |


### `.WriteTmpFile(location, bytes)` 
Returns: `void`

`WriteCustomFile()` but with a file format of '.tmpf', they function the exact same

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`location` |`string` |File path |


### `.ReadCustomFile(path)` 
Returns: `List<byte>`

Reads the list of bytes of the custom image file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`path` |`string` |The list of bytes on the image file |

### `.ReadTmpfFile(path)` 
Returns: `List<byte>`

Reads the list of bytes of the custom tmpf file

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`path` |`string` |The list of bytes on the tmpf file |
