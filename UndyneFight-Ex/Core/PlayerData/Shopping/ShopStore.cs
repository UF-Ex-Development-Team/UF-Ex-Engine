using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.UserService
{
	public enum ItemRarity
	{
		Ordinary = 1,
		Uncommon = 2,
		Rare = 3,
		Epic = 4,
		Legendary = 5
	}
	public abstract class StoreItem(string name, string fullName, bool stackable, string description, ItemRarity rarity, bool inShop, int cost = 0)
	{
		/// <summary>
		/// Whether the item is buyable from the shop (Default true)
		/// </summary>
		public virtual bool InShop { get; set; } = inShop;
		internal bool DefaultInShop { get; } = inShop;
		/// <summary>
		/// The display name of the item
		/// </summary>
		public string Name { get; init; } = name;
		/// <summary>
		/// THe saved name of the item (Affects save file)
		/// </summary>
		public string FullName { get; init; } = fullName;
		/// <summary>
		/// The contributor to the item
		/// </summary>
		public string Contributor { get; init; } = "";
		/// <summary>
		/// Whether the item is stackable
		/// </summary>
		public bool Stackable { get; init; } = stackable;
		/// <summary>
		/// The description of the item in shop
		/// </summary>
		public string Description { get; init; } = description;
		/// <summary>
		/// The description displayed at the user UI (Default <see cref="Description"/>)
		/// </summary>
		public string DisplayDescription { get; init; } = description;
		/// <summary>
		/// The unlock requirement of the item that will be shown in the shop when locked
		/// </summary>
		public string UnlockRequirement { get; init; }
		/// <summary>
		/// The rarity of the item
		/// </summary>
		public ItemRarity Rarity { get; init; } = rarity;
		/// <summary>
		/// The icon of the item
		/// </summary>
		public Texture2D Image { get; set; }
		/// <summary>
		/// The count of the item you have if it's stackable (Default 0)
		/// </summary>
		public int Count { get; set; } = 0;
		/// <summary>
		/// The cost of the item to buy in shop
		/// </summary>
		public int Cost = cost;
		/// <summary>
		/// Whether the item is currently activated
		/// </summary>
		public bool Activated = false;
		/// <summary>
		/// Whether the item is currently being used
		/// </summary>
		public bool Affecting = false;
		/// <summary>
		/// The percentage of the score to reduce if <see cref="ItemAttribute.ReduceScore"/> is true
		/// </summary>
		public float ReducePercentage = 0;
		/// <summary>
		/// Whether the item will automatically be removed from the inventory when chart finishes
		/// </summary>
		public bool Disposable = false;
		/// <summary>
		/// A dictionary of variables you can use for customization
		/// </summary>
		public Dictionary<string, object> Vars { get; set; } = [];
		/// <summary>
		/// The text of the current mode of the item, only modify this if <see cref="ItemAttribute.Cycle"/> is true
		/// </summary>
		public string ModeText = "Default Mode";
		/// <summary>
		/// The attributes of the item
		/// </summary>
		public ItemAttribute Attributes = ItemAttribute.None;
		/// <summary>
		/// Attributes to the item
		/// </summary>
		[Flags]
		public enum ItemAttribute
		{
			/// <summary>
			/// No attribute
			/// </summary>
			None = 0,
			/// <summary>
			/// The item serves as a decoration only
			/// </summary>
			Decoration = 1,
			/// <summary>
			/// The item can be used during charts
			/// </summary>
			Consumable = 2,
			/// <summary>
			/// The item is actually a memory (And should NOT have <see cref="Consumable"/> set to true)
			/// </summary>
			Memory = 4,
			/// <summary>
			/// Whether the score of the chart will be voided when used
			/// </summary>
			VoidScore = 8,
			/// <summary>
			/// Whether the score of the chart will be reduced when used
			/// </summary>
			ReduceScore = 16,
			/// <summary>
			/// Whether the <see cref="InitializeItem"/> will run
			/// </summary>
			Initialize = 32,
			/// <summary>
			/// Whether the item has multiple modes and can be cycled in the menu
			/// </summary>
			Cycle = 64,
		}
		/// <summary>
		/// The trigger condition of the item (Will run constantly through the chart)
		/// </summary>
		/// <returns>Whether the item should be triggered</returns>
		public virtual bool TriggerCondition() => false;
		/// <summary>
		/// Initializes the item
		/// </summary>
		public virtual void InitializeItem() { }
		/// <summary>
		/// The effect of the item when used if <see cref="ItemAttribute.Consumable"/> is true
		/// </summary>
		public virtual void Used() { }
		/// <summary>
		/// The function for items to execute during charts if <see cref="ItemAttribute.Decoration"/> is true
		/// </summary>
		public virtual void Decoration() { }
		/// <summary>
		/// The function to execute when the item is being cycled if <see cref="ItemAttribute.Cycle"/> is true
		/// </summary>
		public virtual void Cycling() { }
		/// <summary>
		/// If <see cref="InShop"/> is set to false, this function will be the validation check for whether the item is unlocked
		/// </summary>
		/// <param name="data">The chart result data</param>
		public virtual bool ValidateItem(SongPlayData data) => InShop;
		/// <summary>
		/// If <see cref="ItemAttribute.VoidScore"/> is true, this function will serve as the additional check
		/// </summary>
		/// <returns>Whether the score will be voided</returns>
		public virtual bool VoidScoreCheck() => true;
	}
	public class StoreData : ISaveLoad
	{
		/// <summary>
		/// The list of all items available for the user to buy
		/// </summary>
		public static Dictionary<string, StoreItem> AllItems { get; } = [];
		/// <inheritdoc/>
		public List<ISaveLoad> Children => null;
		/// <summary>
		/// The list of items the user has unlocked
		/// </summary>
		public static Dictionary<string, StoreItem> UserItems { get; set; } = [];
		/// <summary>
		/// Adds an item to the list of items the user can buy
		/// </summary>
		/// <param name="item">The item to add</param>
		public static void AddToItemList(StoreItem item) => AllItems.TryAdd(item.FullName, item);
		/// <summary>
		/// Consumes an item
		/// </summary>
		/// <param name="itemName">The name of the item to consume (Use <see cref="StoreItem.FullName"/>)</param>
		/// <param name="count">The amount of the item to consume (Default 1)</param>
		/// <returns>Whether the item usage was successful</returns>
		public static bool ConsumeItem(string itemName, int count = 1)
		{
			if (!UserItems.TryGetValue(itemName, out StoreItem item) || item.Count < count)
				return false;
			item.Count -= count;
			return true;
		}
		/// <summary>
		/// Consumes an item
		/// </summary>
		/// <param name="item">The item to consume</param>
		/// <param name="count">The amount of the item to consume (Default 1)</param>
		/// <returns>Whether the item usage was successful</returns>
		public static bool ConsumeItem(StoreItem item, int count = 1) => ConsumeItem(item.FullName, count);
		/// <inheritdoc/>
		public void Load(SaveInfo info)
		{
			foreach (SaveInfo itemInfo in info.Nexts.Values)
			{
				if (!AllItems.ContainsKey(itemInfo.Title))
					continue;
				StoreItem item = AllItems[itemInfo.Title];
				if (item.InShop = itemInfo["unlocked"] is "true" or "True")
				{
					UserItems.TryAdd(item.FullName, item);
					UserItems[item.FullName].InShop = true;
					UserItems[item.FullName].Count = item.Count = (int)MathUtil.FloatFromString(itemInfo["count"]);
					UserItems[item.FullName].Activated = item.Activated = itemInfo["activated"] is "true" or "True";
					try
					{
						if ((item.Attributes & StoreItem.ItemAttribute.Cycle) != 0)
							UserItems[item.FullName].ModeText = item.ModeText = itemInfo["mode"];
					}
					catch { }
				}
			}
		}
		/// <inheritdoc/>
		public SaveInfo Save()
		{
			SaveInfo result = new("StoreData{");
			foreach (StoreItem item in UserItems.Values)
			{
				string info = $"{item.FullName}:count={item.Count},unlocked={item.InShop || item.DefaultInShop},activated={item.Activated}";
				if ((item.Attributes & StoreItem.ItemAttribute.Cycle) != 0)
					info += $",mode={item.ModeText}";
				result.PushNext(new SaveInfo(info));
			}
			return result;
		}
	}
}