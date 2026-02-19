# Setting Up

## Installing MSVS + UF-Ex
Notes
You can either use [Visual Studio Code or Visual Studio](https://visualstudio.microsoft.com/downloads/) to use this engine, however this tutorial only covers Visual Studio, if you intend to use Visual Studio Code, you might need to figure some aspects out yourself.
The documentation may be updated over time, if you are using the PDF version of it, remember to check the site occasionally for the latest changes.

## Requirements
Microsoft Visual Studio (MSVS)

Visual C++ Redistributable Packages for Visual Studio 2013 (Install this first)

.NET 9.0 (C# 13 or higher)

UF-Ex engine pack (UndyneFight-Ex.dll)

UF-Ex .xml file (It is used for variable documentation)

UF-Ex content pack (Included in the example solution)

8 GB of disk space, 4 GB of RAM

The courage to ask for help.

## Installing Visual Studio
Visual Studio Community is sufficient for this engine, you don’t have to install Professional or Enterprise.

You have to install .NET desktop development and WinUI application development in order for the UF-Ex engine to function.

<img width="905" height="229" alt="image" src="/images/Getting Started/Net Desktop.png" />

VS extension development is optional.

<img width="905" height="222" alt="image" src="/images/Getting Started/VS Extension.png" />

Setting up the solution
After installing MSVS, open the provided example solution, you can name it whatever you like.
Open the solution (.sln), select “Extensions” at the top bar, and then choose “Manage Extensions”

<img width="408" height="159" alt="image" src="/images/Getting Started/Manage Extensions.png" />

Choose it and search for “Monogame”

<img width="898" height="435" alt="image" src="/images/Getting Started/Monogame Search.png" />

Install the extension shown in the image. (Note that the extension version in the image is outdated, ensure you have the latest version installed)

After installing Monogame Framework, search for “HLSL”

<img width="884" height="273" alt="image" src="/images/Getting Started/HLSL Search.png" />

Install the extension shown in the image.
(You can choose to not install this if you can read HLSL shader codes in pure text.)

Note that you may need to restart your computer/MSVS after installing certain components.

After finishing the installation process, open MSVS and go to the solution explorer on the right hand side, Right click “Dependencies” and select “Add project reference”

<img width="415" height="74" alt="image" src="/images/Getting Started/Proj Ref.png" />

Click “Browse” and select UndyneFight_Ex.dll.

## Updating UF-Ex
Since UF-Ex depends on Monogame to function, it should be expected that the dependencies will be updated as well (It will be indicated in the changelog). If the dependencies are updated, you should do the following.

<img width="900" height="344" alt="image" src="/images/Getting Started/Folder 1.png" />

In this folder, open the Terminal and run this command “dotnet clean; dotnet restore”.

Then go into this folder.

<img width="900" height="452" alt="image" src="/images/Getting Started/Folder 2.png" />

And open the Terminal here, and run “dotnet tool restore”.

After running them, Monogame should be up to date.
Please make sure you have all the fonts in “Content/Sprites/font” installed, or else the content builder will not recognize the fonts.

## Importing music
By pressing (Ctrl +) F5, the game should run normally (And then you will see a hideous UI).
(Ctrl + F5 will not attach the debugger to the executable, it will run faster but will not show an error log when the game crashes.)
If you try to select a song, you should see this.

<img width="908" height="458" alt="image" src="/images/Getting Started/Import No Music.png" />

A bracket indicates that there is no associated music file for the chart, that means you need to import it.
To import assets, select “Content.mgcb” in the “Content” folder

<img width="429" height="314" alt="image" src="/images/Getting Started/Content Folder.png" />

You can just double click it or choose “Open With -> MGCB Editor”.
**If there is no such folder named “Musics”**, create one by right clicking “Content”, and select “Add -> New Folder” and name it “Musics”.

<img width="807" height="145" alt="image" src="/images/Getting Started/MGCB Editor.png" />

After having the “Musics” folder, create a folder that matches the name of the chart, in this case, it would be “My First Chart”.
Right click on the newly created folder and select “Add -> Existing Item”, then you can select the music you want to import, make sure it says  and the imported music is set to a song .
After importing the music, rename the music asset into “song.(file type)” (If the music you imported was an .ogg file, name it to song.ogg, and so on)
If you want to add a chart cover, simply import an image file and rename it into “paint”.

Note: The engine has auto scaling for chart covers, but it is best to set it to 640x480 before importing to avoid bugs.

After importing the chart assets, make sure to “Build” the MGCB by pressing F6, it is best to use “Rebuild” if you have removed or renamed assets.

## Basic template information
Some comments are inside “Normal Model” and “Championship Model”, you may view them for further information.

```csharp
class MyFirstChart : WaveConstructor, IWaveSet
```

`WaveConstructor` contains the basic methods for charting such as beat calculation and the main chart making function.
`IWaveSet` is the [interface](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface) that stores the basic information of the chart, such as the difficulties and music name.

```csharp
public MyFirstChart() : base(62.5f / (100 / 60f)) { }
```

The “100” is the BPM of the chart, if you are unsure of the BPM, you can use an online BPM checker or use Malody (More accurate) to find the BPM of the music.
If there are multiple BPM in the chart, simply put them in arrays like this:

```csharp
public MyFirstChart() : base([10, 300], [10, 50], [999, 230]) { }
```

This means that the first 10 beats have 300 bpm, the next 10 beats have 50 bpm, and the next 999 beats have 230 bpm.

# [IWaveSet (Classic)](#tab/IWaveSet)

For setting up the difficulties in the most straight forward way, you can set it up like this.
```csharp
//This is the chart function for Noob mode
public void Noob()
{

}
//This is the chart function for Easy mode
public void Easy()
{

}
//This is the chart function for Normal mode
public void Normal()
{

}
//This is the chart function for Hard mode
public void Hard()
{

}
//This is the chart function for Extreme mode
public void Extreme()
{

}
//This is the chart function for Extreme Plus mode
public void ExtremePlus()
{
        
}
```
Note that you can remove the difficulty functions that are not included in your chart.

# [IWaveSetS (Simplified)](#tab/IWaveSetS)

Sometimes, you may find that you need to copy and paste chunks of code from one difficulty to another, and when you need to change certain aspects of the code, you need to manually change them all. Not only is this process boring and tedious, it makes the code unreadable and too large. In short, this is poor coding practice and should be avoided.

To simplify the charting process, IWaveSetS is created, your code can now look as simple as this
```csharp
public void Chart()
{
    Effect();
    switch (CurrentDifficulty)
    {
        case Difficulty.Noob:
            //Noob mode
            break;
        case Difficulty.Normal:
            //Normal mode
            break;
    }
}
```

Or even this

```csharp
public void Chart()
{
    if (InBeat(0))
    {
        int spd = CurrentDifficulty switch
        {
            Difficulty.Noob => 4,
            Difficulty.Easy => 5,
            Difficulty.Normal => 6
        }
        CreateChart(0, BeatTime(1), spd, ["R", "", "", "etc"]);
    }
}
```

Not only is the code more efficient, it is more readable and easy to keep track of.

---

This sums up how to set up a chart, for functions and variables, check out the next chapter.
