namespace UndyneFight_Ex
{
    public class FightSet(string fightSetName)
    {
        private readonly Dictionary<string, Type> fightDictionary = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Type type) => fightDictionary.Add(type.Name, type);

        public Type[] Values => [.. fightDictionary.Values];

        public Type this[string index] => fightDictionary[index];

        public string FightSetName { get; } = fightSetName;
    }
    public class SongSet(string songSetName)
    {
        private readonly Dictionary<string, Type> fightDictionary = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Type type)
        {
            if (fightDictionary.ContainsKey(type.FullName))
                return;
            fightDictionary.Add(type.FullName, type);
        }
        public void Remove(Type type) => fightDictionary.Remove(type.FullName);

        public Type[] Values => [.. fightDictionary.Values];

        public Type this[string index] => fightDictionary[index];

        public string SongSetName { get; } = songSetName;
    }
}