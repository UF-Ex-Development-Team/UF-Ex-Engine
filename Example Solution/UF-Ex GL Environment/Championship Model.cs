using System.Collections.Generic;
using UndyneFight_Ex;
using UndyneFight_Ex.SongSystem;

namespace UF_Ex_Environment
{
	internal class ChampionshipChart : IChampionShip
	{
		public Dictionary<string, Difficulty> DifficultyPanel => new()
		{
			//This adds Extreme mode to the chart as "div 1"
			["div 1"] = Difficulty.Extreme,
			//This adds Noob mode to the chart as "div 2"
			["div 2"] = Difficulty.Noob,
		};
		public IWaveSet GameContent => new Project();
		//IWaveSetS is also a valid interface to avoid mass copying and pasting of codes
		class Project() : WaveConstructor(120, true), IWaveSetS
		{
			public string Music => "";	
			public string FightName => "";
			public SongInformation Attributes => new Information();
			class Information : SongInformation
			{
				public override string SongAuthor => "";
				public override string BarrageAuthor => "";
				public override string AttributeAuthor => "";
				public override Dictionary<Difficulty, float> CompleteDifficulty => new()
				{
					[Difficulty.Noob] = 0,
					[Difficulty.Extreme] = 0
				};
				public override Dictionary<Difficulty, float> ComplexDifficulty => new()
				{
					[Difficulty.Noob]  = 0,
					[Difficulty.Extreme] = 0
				};
				public override Dictionary<Difficulty, float> APDifficulty => new()
				{
					[Difficulty.Noob] = 0,
					[Difficulty.Extreme] = 0
				};
			}
			public new void Start()
			{

			}
			public void Chart()
			{

			}
		}
	}
}
