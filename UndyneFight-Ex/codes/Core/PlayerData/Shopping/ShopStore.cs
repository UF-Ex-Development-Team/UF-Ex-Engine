using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.IO;

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
    public abstract class StoreItem(string name, string fullName, bool stackable, string description, ItemRarity rarity, int count, int cost = 0)
    {
        /// <summary>
        /// Whether the item is buyable from the shop
        /// </summary>
        public virtual bool InShop { get; } = true;
        /// <summary>
        /// The display name of the item
        /// </summary>
        public string Name { get; init; } = name;
        /// <summary>
        /// THe saved name of the item (Affects save file)
        /// </summary>
        public string FullName { get; init; } = fullName;
        /// <summary>
        /// Whether the item is stackable
        /// </summary>
        public bool Stackable { get; init; } = stackable;
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description { get; init; } = description;
        /// <summary>
        /// The rarity of the item
        /// </summary>
        public ItemRarity Rarity { get; init; } = rarity;
        /// <summary>
        /// The icon of the item
        /// </summary>
        public Texture2D Image { get; set; }
        /// <summary>
        /// The count of the item you have if it's stackable
        /// </summary>
        public int Count { get; set; } = count;
        /// <summary>
        /// The cost of the item to buy in shop
        /// </summary>
        public int Cost = cost;
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
            /// The item can be viewed in the account UI
            /// </summary>
            Viewable = 1,
            /// <summary>
            /// The item can be used
            /// </summary>
            Consumable = 2,
        }

        public virtual void Selected()
        {

        }
        public virtual void DeSelected()
        {

        }
        public virtual void Used()
        {

        }
    }
    public class StoreData : ISaveLoad
    {
        /// <summary>
        /// The list of all items available
        /// </summary>
        public static Dictionary<string, StoreItem> AllItems { get; } = [];
        /// <inheritdoc/>
        public List<ISaveLoad> Children => null;
        /// <summary>
        /// The list of items the user has
        /// </summary>
        public Dictionary<string, StoreItem> userItems = [];
        public static void AddToItemList(StoreItem item) => AllItems.TryAdd(item.FullName, item);
        /// <summary>
        /// Consumes an item
        /// </summary>
        /// <param name="itemName">The name of the item to consume (Use <see cref="StoreItem.FullName"/>)</param>
        /// <param name="count">The amount of the item to consume (Default 1)</param>
        /// <returns>Whether the item usage was successful</returns>
        public bool ConsumeItem(string itemName, int count = 1)
        {
            if (!userItems.TryGetValue(itemName, out StoreItem item))
                return false;
            if (item.Count < count)
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
        public bool ConsumeItem(StoreItem item, int count = 1) => ConsumeItem(item.FullName, count);
        /// <inheritdoc/>
        public void Load(SaveInfo info)
        {
            foreach (SaveInfo itemInfo in info.Nexts.Values)
            {
                StoreItem item = AllItems[itemInfo.Title];
                userItems.Add(item.FullName, item);
                item.Count = itemInfo.IntValue;
            }
        }
        /// <inheritdoc/>
        public SaveInfo Save()
        {
            SaveInfo result = new("StoreData{");
            foreach (StoreItem item in userItems.Values)
                result.PushNext(new SaveInfo($"{item.FullName}:value={item.Count}"));
            return result;
        }
    }
}