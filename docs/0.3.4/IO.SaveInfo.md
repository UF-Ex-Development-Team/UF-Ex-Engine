# IO.SaveInfo

### `SaveInfo` (*class*)

Storage rules of `SaveInfo`:

[] are constants，{} are an array of `values`, ->{} are an array of `Nexts`, `,` is the seperator of data

Examples are as follows:

(ChampionShips)->{div=[string],score=[int],position=[int]}

PlayerName:[string],VIP:[bool]

NormalFight->{(songName):noob=[int],easy=[int],...extreme=[int]}

Achievements->{(achievementName):type=[bool],progress=[int]}

Creation:
Creates a save info with the given key

| Parameter | Datatype  | Purpose |
|-----------|-----------|---------|
|`val` |`string` |The key and or value of the save info |


### `PushNext(info)` 
Returns: `void`

Adds nested SaveInfo in `Nexts`
