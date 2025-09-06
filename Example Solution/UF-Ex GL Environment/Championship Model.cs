using System.Collections.Generic;
using UndyneFight_Ex;
using UndyneFight_Ex.SongSystem;

namespace UF_Ex_Environment
{
	internal class ChampionshipChart : IChampionShip
	{
		public Dictionary<string, Difficulty> DifficultyPanel => new()
		{
			["div 1"] = Difficulty.ExtremePlus
		};
		public IWaveSet GameContent => new Project();
		class Project() : WaveConstructor(120, true), IWaveSet
		{
			public string Music => "";	
			public string FightName => "";
			public SongInformation Attributes => new Information();
			class Information : SongInformation
			{
				public override string SongAuthor => "";
				public override string BarrageAuthor => "";
				public override string AttributeAuthor => "";
				public override Dictionary<Difficulty, float> CompleteDifficulty => new(
						[
							new(Difficulty.Noob, 0),
							new(Difficulty.Extreme, 0)
						]
					);
				public override Dictionary<Difficulty, float> ComplexDifficulty => new(
						[
							new(Difficulty.Noob, 0),
							new(Difficulty.Extreme, 0)
						]
					);
				public override Dictionary<Difficulty, float> APDifficulty => new(
						[
							new(Difficulty.Noob, 0),
							new(Difficulty.Extreme, 0)
						]
					);
			}
			public new void Start()
			{

			}
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
}
