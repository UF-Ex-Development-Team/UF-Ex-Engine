using static System.Math;
using static System.MathF;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Entities.Particle;
using static UndyneFight_Ex.FightResources.Sounds;

namespace UndyneFight_Ex.Entities
{
    public partial class Player
    {
        public class HPControl : GameObject
        {
            private Protected<float> hp;

            private static bool Buffed => ((CurrentScene as FightScene).Mode & GameMode.Buffed) != 0;
            private float missionLostSpeed = 0.005f;
            private float curLost = 0.01f;
            /// <summary>
            /// The losing speed of the HP
            /// </summary>
            internal float LostSpeed => Pow(curLost * 100, 0.7f);
            internal float Under1HPScale => MathF.Max(0, 1 - hp / maxHP * 5);
            /// <summary>
            /// The damage taken per hit
            /// </summary>
            public int DamageTaken { internal get; set; } = 1;

            internal float maxHP = 5;
            internal float HP { get => hp.Value; set => hp.Value = value; }
            /// <summary>
            /// The invincibility frames
            /// </summary>
            public int protectTime = 0;
            /// <summary>
            /// Whether KR is enabled
            /// </summary>
            public bool KR { get; set; } = false;
            /// <summary>
            /// Whether the soul is invincible to attacks
            /// </summary>
            public bool InvincibleToPhysic { get; set; } = false;
            /// <summary>
            /// The damage KR deals
            /// </summary>
            public float KRDamage { get; set; } = 4;
            /// <summary>
            /// Whether there exists KR hp
            /// </summary>
            public bool KRHPExist => KRHP > 0.0005f;
            /// <summary>
            /// Adds KR hp to the player
            /// </summary>
            /// <param name="intensity">The intensity of the KR to apply (<see cref="KRDamage"/> * <paramref name="intensity"/>)</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GiveKR(float intensity) => KRHP += KRDamage * intensity;
            /// <summary>
            /// The amount of KR hp
            /// </summary>
            public float KRHP { get; private set; } = 0;
            private static bool NoHIT => ((CurrentScene as FightScene).Mode & GameMode.NoHit) != 0;
            /// <summary>
            /// The buffed difficulty of the player if buffed mode is enabled (<see cref="BuffedLevel"/> + <see cref="BuffDifficulty"/>)
            /// </summary>
            public float BuffDifficulty { get; set; } = 4;
            /// <summary>
            /// The buffed level of the player
            /// </summary>
            public float BuffedLevel { get; set; } = 0;
            /// <summary>
            /// Whether the HP can exceed <see cref="maxHP"/>
            /// </summary>
            public bool OverFlowAvailable { get; set; } = false;
            /// <summary>
            /// Whether score will not be counted during invincibility frames
            /// </summary>
            public bool ScoreProtected { get; set; } = false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void GetMark(int mark)
            {
                if (mark <= 1)
                    missionLostSpeed = missionLostSpeed * 0.8f + 0.2f * 0.2f;
                if (mark == 2)
                    missionLostSpeed = missionLostSpeed * 0.85f + 0.12f * 0.15f;
                if (mark == 3)
                    missionLostSpeed *= 0.965f;
                if (mark >= 4)
                    missionLostSpeed = missionLostSpeed < 0.05f ?
                        (missionLostSpeed * 0.95f + 0.05f * 0.1f) :
                        missionLostSpeed;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DoHPLose()
            {
                if (KRHP > hp)
                    KRHP = hp;

                float del = KRHP * 0.004f;
                float krLose = 0;
                krLose += del;
                KRHP -= del;
                float lose2 = Math.Min(0.004f, KRHP);
                krLose += lose2;
                KRHP -= lose2;
                hp.Value -= krLose;
            }
            /// <summary>
            /// Applies the invincibility frames to the heart
            /// </summary>
            /// <param name="val">The duration of the invincibility frames</param>
            /// <param name="ProtectScore">Whether Nice/Okay collision will take place during the inv. frames</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GiveProtectTime(int val, bool ProtectScore = false)
            {
                protectTime = val * 2;
                ScoreProtected = ProtectScore;
            }
            /// <summary>
            /// Damages the target heart (Apply particle effect to that heart)
            /// </summary>
            /// <param name="heart">The heart to damage</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LoseHP(Heart heart)
            {
                if (protectTime > 0)
                    return;

                protectTime = !KR ? 110 : 5;

                bool NoGreenSoul = ((CurrentScene as FightScene).Mode & GameMode.NoGreenSoul) != 0;
                if (!NoGreenSoul || (heart.SoulType != 1 && NoGreenSoul))
                {
                    if (!InvincibleToPhysic)
                        hp.Value -= DamageTaken;
                    if (KR)
                        KRHP += DamageTaken;
                }
                playerHurt.CreateInstance().Play();
                CreateParticles(new Color(140, 0, 0, 100), 2f, 8f, heart.Centre, KR ? 4 : 16, 4);

                CreateParticles(Color.Lerp(heart.CurrentMoveState.StateColor * 0.4f, new Color(100, 0, 0, 100), 0.5f), 2f, 8f, heart.Centre, 6, 4);
            }
            /// <summary>
            /// Sets KR to 0
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetKR() => KRHP = 0;
            /// <summary>
            /// Sets Max HP to 5
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetHP() => ResetMaxHP(5);
            /// <summary>
            /// Sets the HP value
            /// </summary>
            /// <param name="hpCnt">The HP value to set to</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetHP(int hpCnt) => hp.Value = hpCnt;
            /// <summary>
            /// Sets the Max HP value and sets the HP to that value
            /// </summary>
            /// <param name="hpCnt">The Max HP to set to</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetMaxHP(float hpCnt)
            {
                maxHP = hpCnt;
                hp.Value = NoHIT ? (maxHP = 1) : maxHP;
                KRHP = 0;
            }
            /// <summary>
            /// Full heal
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Regenerate()
            {
                KRHP = 0;
                hp.Value = maxHP;

                missionLostSpeed = MathF.Min(missionLostSpeed, 0.005f);
            }
            /// <summary>
            /// Heal for the given value
            /// </summary>
            /// <param name="hp_">The amount of HP to recover</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Regenerate(int hp_)
            {
                hp.Value = OverFlowAvailable ? hp + hp_ : Math.Min(hp + hp_, maxHP);
                KRHP = MathF.Max(0, MathF.Min(KRHP, maxHP - hp));

                missionLostSpeed = MathF.Min(missionLostSpeed, 0.005f);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void Update()
            {
                if (protectTime > 0)
                    protectTime--;
                if (protectTime == 0)
                    ScoreProtected = false;
                curLost = MathHelper.Lerp(curLost, missionLostSpeed, 0.05f);
                missionLostSpeed *= 0.9995f;
                if (Buffed || BuffedLevel != 0)
                {
                    float scale = 1;
                    if (!NoHIT)
                        scale = MathF.Min(1, hp / maxHP * 5 * 0.8f + 0.2f);
                    float scale2 = hp.Value / maxHP;
                    float recovery = MathUtil.Clamp(0, 0.03f - scale2 * 0.03f, 0.01f);
                    float dif = BuffedLevel + (Buffed ? BuffDifficulty : 0);
                    hp.Value -= maxHP * (missionLostSpeed - recovery * 6.5f / dif) * 0.0014f * dif * scale;
                }
                if (KR && KRHP > 0)
                    DoHPLose();

                if (hp.Hacked)
                    GameStates.CheatAffirmed();

                bool PracticeDisabled = ((CurrentScene as FightScene).Mode & GameMode.Practice) == 0;
                if (hp <= 0)
                {
                    if (PracticeDisabled)
                    {
                        GameMain.instance.SetGameoverScreen();
                        (CurrentScene as FightScene).PlayDeath();
                        return;
                    }
                    else if (!(CurrentScene as SongFightingScene).HPReached0)
                        (CurrentScene as SongFightingScene).HPReached0  = true;
                }
            }

            public HPControl()
            {
                hp.Value = 5;
                UpdateIn120 = true;
            }
        }
    }
}