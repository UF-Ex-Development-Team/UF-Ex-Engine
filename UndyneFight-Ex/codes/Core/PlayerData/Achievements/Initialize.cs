using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService
{
    public partial class AchievementManager : ISaveLoad
    {
        public List<ISaveLoad> Children => [];

        public Dictionary<string, AchievementObject> AchievementObjects = [];

        public void Load(SaveInfo info)
        {
            foreach (KeyValuePair<string, SaveInfo> pair in info.Nexts)
            {
                if (Achievements.AchievementManager.achievements.TryGetValue(pair.Key, out Achievement value))
                { 
                    Insert(value);
                    AchievementObjects[pair.Key].Load(pair.Value);
                }
            }
            foreach (KeyValuePair<string, Achievement> achieve in Achievements.AchievementManager.achievements)
            {
                if (!AchievementObjects.ContainsKey(achieve.Key))
                    Insert(achieve.Value);
            }
        }
        public void Insert(Achievement achievement)
        {
            AchievementObject obj = new(achievement);
            AchievementObjects.TryAdd(achievement.Title, obj);
        }

        public SaveInfo Save()
        {
            SaveInfo info = new("Achievements{");
            foreach (KeyValuePair<string, AchievementObject> v in AchievementObjects)
            {
                info.PushNext(v.Value.Save());
            }
            return info;
        }
    }
}