# Basic Charting

Now that you have finished setting up the environment, you can start making charts! These are the most commonly used variables/classes.
- Heart
- HeartAttribute
- UndyneFight_Ex.Fight.Functions

## Chart Initialization

First, you should initialize data for your chart, you can do that by using `Start()`, here is an example.
```csharp
public new void Start()
{
    HeartAttribute.MaxHP = 8; //Sets the max HP to 8
    HeartAttribute.Speed = 4; //Sets the movement speed to 4 pixels per frame
    HeartAttribute.SoftFalling = true; //Smothen the blue soul falling logic
    SetGreenBox(); //Sets the box into the default green soul box (Position at (320, 240), size set to (84, 84))
    TP(); //Teleports the soul to (320, 240)
    SetSoul(1); //Sets the soul to a green soul
    GametimeDelta = -120; //Sets the initial game time to be -120 instead of the default 0
    PlayOffset = GametimeDelta + 120; //Sets the play offset of the music to be 0 (in this case)
}
```
## Time measurement

After initializing the chart, the next step would be to actaully do anything in the chart. The most immediate thing to do would be to figure out how to time anything, here are a few ways to measure time in UF-Ex:
- GametimeF (Frames elapsed in float)
- BeatTime(X) (Duration of X beats in frames)
- InBeat(X) (Whether the chart is at the Xth beat)

Note that since `GametimeF` is always a multiple of 0.5 and `BeatTime` can be any value like 12.284671, you cannot perform this
```csharp
if (GametimeF == BeatTime(10))
{
    //Code
}
```
Therefore, it is best to have events set up like this:

```csharp
if (InBeat(1))
{
    //Code
}
else if (InBeat(10))
{
    //Code
}
```

Let's say for example, you would like to increase the screen scale by 0.2 **every 2 beats from the 6th beat to the 12th beat**, while you can use the following code:
```csharp
if (InBeat(6) || InBeat(8) || InBeat(10) || InBeat(12))
    ScreenDrawing.ScreenScale += 0.2f;
```
You can see that it is going to get very messy if it gets more intensive, therefore, you can use the following format instead:
```csharp
if (InBeat(6, 12) && At0thBeat(2)) //Checks if the current time is between the 6th and 12th beat **and** if the current beat is a multiple of 2
    ScreenDrawing.ScreenScale += 0.2f;
```
However, there are occasions that events need to happen on a set delay after the multiple of a beat, for example:
```csharp
if (InBeat(7) || InBeat(9) || InBeat(11) || InBeat(13))
    ScreenDrawing.ScreenScale += 0.2f;
```
As you may have noticed, the effect should be the same as the one above, though shifted by 1 beat, so similiary, we can use `AtKthBeat` to simplify the process:
```csharp
if (InBeat(6, 12) && AtKthBeat(2, BeatTime(1))) //Checks if the current time is between the 6th and 12th beat **and** if the current beat is offset by 1 beat of any beat that is a multiple of 2
    ScreenDrawing.ScreenScale += 0.2f;
```
> [!WARNING]
> Remember that the second argument `AtKthBeat` represents the frame delay, not the beat delay, so you have to input `BeatTime(1)` for 1 beat, not just `1`.

## Executing events
If you had read older iterations (v0.1.6) of the UF-Ex documentation, you might know that you can create arrows by using the following format of code
```csharp
if (InBeat(1)) //At the first beat
{
    time = BeatTime(10);
    CreateArrow(time, "R", 6.4f, 0, 0);                 //Have an arrow from a random direction with the speed of 6.4 pixels per frame to reach the shield in 10 beats
    CreateArrow(time += BeatTime(2), "R", 6.4f, 0, 0);  //Have an arrow from a random direction with the speed of 6.4 pixels per frame to reach the shield in 12 beats
    CreateArrow(time += BeatTime(2), "R", 6.4f, 0, 0);  //Have an arrow from a random direction with the speed of 6.4 pixels per frame to reach the shield in 14 beats
    CreateArrow(time += BeatTime(2), "R", 6.4f, 0, 0);  //Have an arrow from a random direction with the speed of 6.4 pixels per frame to reach the shield in 16 beats
    CreateArrow(time += BeatTime(2), "R", 6.4f, 0, 0);  //Have an arrow from a random direction with the speed of 6.4 pixels per frame to reach the shield in 18 beats
}
```
We don't do that anymore. We do this instead.
```csharp
if (InBeat(1))
{
    //In 10 beats, execute all the events represented by the strings
    //4 beats pass for every 8 string entries (Therefore we usually have a line break every 8 string entries)
    //All arrows will move at 6.4 pixels per frame by default
    CreateChart(BeatTime(10), BeatTime(4), 6.4f, [
        "R", "", "", "", "R", "", "", "",
        "R", "", "", "", "R", "", "", "",
        "R", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "",
    ]);
}
```

Let's say again, that you would increase the screen scale by 0.2 every 2 beats from the 6th beat to the 12th beat **in addition to** the 5 random arrows from before, the code would look like this:
```csharp
if (InBeat(1))
{
    //Stores a tempoary function called "IncScale" that increases the screen scale by 0.2
    RegisterFunctionOnce("IncScale", () => ScreenDrawing.ScreenScale += 0.2f);
    CreateChart(BeatTime(5), BeatTime(4), 6.4f, [
        "IncScale", "", "", "", "IncScale", "", "", "",
        "IncScale", "", "R", "", "IncScale", "", "R", "",
        "R", "", "", "", "R", "", "", "",
        "R", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "",
    ]);
}
```
There are a lot of occasions that you might want to stack events, you can do that use brackets to combine them together:
```csharp
CreateChart(BeatTime(4), BeatTime(1), 7, [
    "R", "", "(IncScale)(R)", "", "", "", "", "",  //1 random blue arrow, then increase the screen scale and spawn one random blue arrow after 0.25 beats
]);
```
You can read more about its usages in its documentation text.