//These are example usings, you may edit them if you want
using System;
using System.Collections.Generic;
using System.Linq;
using UndyneFight_Ex;
using UndyneFight_Ex.SongSystem;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.Fight;
using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;
//The name of the namespace is purely optional, but make sure it is consistent
namespace UF_Ex_Environment;
//Then you define a class for the chart, the name must be unique
//WaveConstructor and IWaveSet are the base classes for the chart
//The argument inside WaveConstructor is the BPM of the chart, this example is the implementation of multiple BPM in a chart
class MyFirstChart() : WaveConstructor([(20, 100), (20, 200)]), IWaveSet 
{
	//The file path of the music
	public string Music => "My First Chart";
	//The name of the chart (Affects user score as FightName is used for data saving)
	public string FightName => "My First Chart";
		
	private class ThisInformation : SongInformation
	{
		//Setup the difficulties of the chart
		public override Dictionary<Difficulty, float> CompleteDifficulty => new()
		{
			[Difficulty.Noob] = 0,
			[Difficulty.Easy] = 0,
			[Difficulty.Normal] = 0,
			[Difficulty.Hard] = 0,
			[Difficulty.Extreme] = 0
		};
		public override Dictionary<Difficulty, float> ComplexDifficulty => new()
		{
			[Difficulty.Noob] = 0,
			[Difficulty.Easy] = 0,
			[Difficulty.Normal] = 0,
			[Difficulty.Hard] = 0,
			[Difficulty.Extreme] = 0
		};
		public override Dictionary<Difficulty, float> APDifficulty => new()
		{
			[Difficulty.Noob] = 0,
			[Difficulty.Easy] = 0,
			[Difficulty.Normal] = 0,
			[Difficulty.Hard] = 0,
			[Difficulty.Extreme] = 0
		};
		//Name of charter
		public override string BarrageAuthor => "Name";
		//Name of effect maker
		public override string AttributeAuthor => "Name";
		//Name of artist of the cover
		public override string PaintAuthor => "Name";
		//Name of musician
		public override string SongAuthor => "Name";

		//There are other variables in “SongInformation”, such as “Hidden” and “Extra”, you can view their description in the API section or in the code summary after importing the .xml file.
	}
	//If you don't want any information to be displayed, just set this to null
	public SongInformation Attributes => new ThisInformation();
	//This function will be executed when the chart begins
	public new void Start()
	{

	}
	//Below are the functions for each difficulty, you can delete the function if that difficulty is not included

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
}