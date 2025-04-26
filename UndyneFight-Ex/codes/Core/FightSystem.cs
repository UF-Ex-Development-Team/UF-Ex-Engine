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
    /// <summary>
    /// A list of charts
    /// </summary>
    /// <param name="songSetName">The name of the song set</param>
    public class SongSet(string songSetName)
    {
        private readonly Dictionary<string, Type> fightDictionary = [];
        /// <summary>
        /// Add a chart to the song set
        /// </summary>
        /// <param name="type"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Type type) => fightDictionary.TryAdd(type.FullName, type);
        /// <summary>
        /// Remove a chart from the song set
        /// </summary>
        /// <param name="type"></param>
        public void Remove(Type type) => fightDictionary.Remove(type.FullName);
        /// <summary>
        /// The list of charts
        /// </summary>
        public Type[] Values => [.. fightDictionary.Values];
        /// <summary>
        /// Gets the chart with the given index
        /// </summary>
        /// <param name="index">The index of the chart</param>
        /// <returns></returns>
        public Type this[string index] => fightDictionary[index];
        /// <summary>
        /// The name of the song set
        /// </summary>
        public string SongSetName { get; } = songSetName;
    }
}