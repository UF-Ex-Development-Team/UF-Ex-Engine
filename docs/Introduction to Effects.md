# Introduction to Effects
Once basic charting is mastered, it's time to add in some effects to make your chart stand out. Effects can be of wide variety, such as sound effects, screen mainipulation, and shaders.

## Sound Effects
Often, you might want to play sound effects in chart events, you can use `PlaySound` to, well, play a sound. Since all sounds are either in `FightResources` or `GlobalResources`, it is recommended to have them in usings to simplify the process, for example:
```csharp
using static UndyneFight_Ex.FightResources;

//Chart class code

if (InBeat(1))
    PlaySound(pierce);
```

You can also specify the volume for the audio played in the second argument, i.e. `PlaySound(pierce, 0.5f)`.

## Screen Mainipulation
