namespace UndyneFight_Ex.Fight
{
	public class GameAction(string name, Action usingEvent)
	{
		public string Name { internal get; set; } = name;
		public Action UsingEvent { internal get; set; } = usingEvent;
	}
	public class Item(string name, Action usingEvent)
	{
		public string Name { internal get; set; } = name;
		public Action UsingEvent { internal get; set; } = usingEvent;
		public bool IsConsumable { internal get; set; } = true;
	}
}