using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
    public abstract class FightScene : Scene
    {
        /// <summary>
        /// Current player instance in fight
        /// </summary>
        internal Player PlayerInstance { get; set; }
        /// <summary>
        /// The HP Bar in the fight
        /// </summary>
        internal HPShower HPBar { get; set; }
        /// <summary>
        /// The name display in fight
        /// </summary>
        internal NameShower NameShow { get; set; }

        private bool playerAlive = true;
        private readonly CheatDetector Detector = new();

        public abstract GameMode Mode { get; }
        public override void Dispose()
        {
            ResetFightState(!playerAlive);
            base.Dispose();
        }
        public FightScene()
        {
            ResetFightState(false);
            UpdateIn120 = true;
            InstanceCreate(NameShow = new NameShower());
            InstanceCreate(new CheatDetector());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlayDeath()
        {
            playerAlive = false;
            foreach (Arrow bullet in Fight.Functions.GetAll<Arrow>())
                bullet.NoScore = true;
            foreach (Barrage bullet in Fight.Functions.GetAll<Barrage>())
                bullet.MarkScore = false;
            PlayerDied();

            Achievements.AchievementManager.CheckUserAchievements();
            GameStates.InstanceCreate(new Player.BrokenHeart());
        }
        protected abstract void PlayerDied();

        public override void Start() { }

        public override void Update()
        {
            if (stopTime <= 0.01f)
            {
                GasterBlaster.shootSoundPlayed = GasterBlaster.spawnSoundPlayed =
                Pike.shootSoundPlayed = Pike.spawnSoundPlayed = false;

                foreach (Player.Heart heart in Player.hearts)
                {
                    if (heart.SoulType == 1)
                        continue;
                    foreach (GameObject entity in Objects)
                    {
                        if (entity is ICollideAble collideAble)
                            collideAble.GetCollide(heart);
                    }
                }
            }

            base.Update();
        }
    }
    internal class NormalFightingScene : FightScene
    {
        private int appearTime = 0, restartTimer = 0;
        private readonly Fight.IClassicFight current;
        private readonly GameMode mode;
        public override GameMode Mode => mode;

        public NormalFightingScene(Fight.IClassicFight obj, GameMode mode)
        {
            this.mode = mode;
            Type type = obj.GetType();
            current = (Fight.IClassicFight)Activator.CreateInstance(type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void PlayerDied() => ResetScene(new TryAgainScene(current, mode));
        public override void Update()
        {
            if (appearTime == 0)
                Fight.ClassicFightEnterance.CreateClassicFight(current);
            appearTime++;
            base.Update();

            restartTimer = IsKeyDown(InputIdentity.Reset) ? restartTimer + 1 : 0;
            if (restartTimer >= 45)
            {
                ResetFightState(true);
                GameStates.InstanceCreate(new Player.BrokenHeart());
                ResetScene(new TryAgainScene(Fight.ClassicFightEnterance.currentFight, mode));
            }
        }
    }
}