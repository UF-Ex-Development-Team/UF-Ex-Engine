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
//The namespace is purely optional, but make sure it is consistent
namespace UF_Ex_Environment
{
	//Then you define a class for the chart, the name must be unique
	class MyFirstChart : WaveConstructor, IWaveSet //WaveConstructor and IWaveSet are the base classes for the chart
	{
		//Sets the BPM of the chart
		public MyFirstChart() : base([(20, 100), (20, 200)]) { }
		//The file path of the music
		public string Music => "My First Chart";
		//The name of the chart (Affects user score as FightName is used for data saving)
		public string FightName => "My First Chart";
		
		private class ThisInformation : SongInformation
		{
			//Setup the difficulties of the chart
			public override Dictionary<Difficulty, float> CompleteDifficulty => new(
					[
						new(Difficulty.Noob, 0),
						new(Difficulty.Easy, 0),
						new(Difficulty.Normal, 0),
						new(Difficulty.Hard, 0),
						new(Difficulty.Extreme, 0)
					]
				);
			public override Dictionary<Difficulty, float> ComplexDifficulty => new(
					[
						new(Difficulty.Noob, 0),
						new(Difficulty.Easy, 0),
						new(Difficulty.Normal, 0),
						new(Difficulty.Hard, 0),
						new(Difficulty.Extreme, 0)
					]
				);
			public override Dictionary<Difficulty, float> APDifficulty => new(
					[
						new(Difficulty.Noob, 0),
						new(Difficulty.Easy, 0),
						new(Difficulty.Normal, 0),
						new(Difficulty.Hard, 0),
						new(Difficulty.Extreme, 0)
					]
				);
			public override string BarrageAuthor => "Name";
			public override string AttributeAuthor => "Name";
			public override string PaintAuthor => "Name";
			public override string SongAuthor => "Name";
			//If MusicOptimized is set to true, the song speed will be adjusted when the game is slowed down/sped up
			public ThisInformation() { MusicOptimized = false; }
		}
		//If you don't want any information to be displayed, just set this to null
		public SongInformation Attributes => new ThisInformation();
		//This function will be executed when the chart begins
		public new void Start()
		{

		}
		//The following functions are for each difficulty
		public void Noob()
		{
			
		}
		public void Easy()
		{
			
		}
		public void Normal()
		{
			
		}
		public void Hard()
		{
			
		}
		public void Extreme()
		{
			
		}
		public void ExtremePlus()
		{
			
		}
	}
}