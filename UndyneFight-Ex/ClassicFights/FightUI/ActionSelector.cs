namespace UndyneFight_Ex.Fight
{
	internal static class ActionShowing
	{
		internal class ActionSelector : FightTextSelection
		{
			private readonly GameAction action;

			public ActionSelector(string text, float height, GameAction action) : base(text, new vec2(50, height))
			{
				this.action = action;
				controlLayer = Surface.Hidden;
			}

			public override void ZPressed()
			{
				ClassicFight.EndSelecting();
				action.UsingEvent();
			}
		}

		internal static FightSelectionCollection GetActionSelector()
		{
			FightSelection[] fightSelections = new FightSelection[FightStates.actions.Count];
			float curHeight = 256;
			int i = 0;
			foreach (GameAction v in FightStates.actions)
			{
				fightSelections[i] = new ActionSelector("  * " + v.Name, curHeight, v);
				i++;
				curHeight += 40;
			}
			return new FightSelectionCollection(fightSelections) { };
		}
	}
}