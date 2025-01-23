using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService
{
    public partial class ShopData
    {
        public class ShopCash : ISaveLoad
        {
            public int Coins { get; set; }
            public int Energy { get; set; } = 0;
            public int Resonance { get; set; } = 0;
            /// <inheritdoc/>
            public List<ISaveLoad> Children => null;
            /// <inheritdoc/>
            public void Load(SaveInfo info)
            {
                if (info.Nexts.TryGetValue("Coins", out SaveInfo value))
                    Coins = value.IntValue;
                if (info.Nexts.TryGetValue("Energy", out value))
                    Energy = value.IntValue;
                if (info.Nexts.TryGetValue("Resonance", out value))
                    Resonance = value.IntValue;
            }
            /// <inheritdoc/>
            public SaveInfo Save()
            {
                SaveInfo info = new("ShopCash{");
                info.PushNext(new SaveInfo("Coins:value=" + Coins));
                //info.PushNext(new SaveInfo("Energy:value=" + Energy));
                //info.PushNext(new SaveInfo("Resonance:value=" + Resonance));
                return info;
            }
        }
    }
}