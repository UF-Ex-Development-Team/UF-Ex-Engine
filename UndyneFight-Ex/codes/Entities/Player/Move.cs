using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    public partial class Player
    {
        public partial class Heart
        {
            private static class Move
            {
                public static Heart mission;
                private static bool Slow => IsKeyDown(InputIdentity.Cancel);

                private static InputIdentity[] Keys_ => mission.movingKey;// { Keys.Right, Keys.Down, Keys.Left, Keys.Up };

                public static int last = -1;

                public static bool blueLastWay = false;

                public static int currentLine = 0;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static void ClampHeartInBox()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;
                    if (mission.collidingBox.X < _curBox.Left)
                        mission.collidingBox.X = _curBox.Left;
                    else if (mission.collidingBox.Right > _curBox.Right)
                        mission.collidingBox.X = _curBox.Right - 16;
                    if (mission.collidingBox.Y < _curBox.Up)
                        mission.collidingBox.Y = _curBox.Up;
                    else if (mission.collidingBox.Down > _curBox.Down)
                        mission.collidingBox.Y = _curBox.Down - 16;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsPurple()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;

                    if (mission.lastChangeTime >= 3)
                    {
                        mission.purpleLineLength = _curBox.CollidingBox.Width;

                        mission.Centre += _curBox.Centre - mission.lastBoxCentre;
                    }

                    Vector2 moving = Vector2.Zero;

                    if (IsKeyDown(Keys_[2]))
                        moving = -vec2.UnitX;

                    if (IsKeyDown(Keys_[0]))
                        moving = vec2.UnitX;

                    if (IsKeyPressed120f(Keys_[3]))
                        currentLine--;
                    if (IsKeyPressed120f(Keys_[1]))
                        currentLine++;

                    currentLine = Math.Clamp(currentLine, 1, mission.purpleLineCount);

                    float delta = _curBox.CollidingBox.Height / (mission.purpleLineCount + 1);

                    Vector2 delta2 = new(0, delta * currentLine + _curBox.Up - mission.Centre.Y);
                    mission.positionRest.Y = delta2.Y;

                    if (moving != Vector2.Zero)
                        moving *= mission.Speed * (Slow ? 0.5f : 1);

                    mission.collidingBox.Offset(moving * 0.5f);
                    if (mission.collidingBox.X < _curBox.Left)
                        mission.collidingBox.X = _curBox.Left;
                    else if (mission.collidingBox.Right > _curBox.Right)
                        mission.collidingBox.X = _curBox.Right - 16;
                    mission.lastBoxCentre = _curBox.Centre;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsOrange()
                {
                    for (int i = 0; i < 4; i++)
                        if (IsKeyDown(Keys_[i]))
                        {
                            last = i;
                            break;
                        }
                    for (int i = 0; i < 4; i++)
                        if (IsKeyDown(Keys_[i]) && IsKeyDown(Keys_[(i + 1) % 4]))
                        {
                            last = 4 + i;
                            break;
                        }

                    Vector2 moving = last switch
                    {
                        0 => Vector2.UnitX,
                        1 => Vector2.UnitY,
                        2 => -Vector2.UnitX,
                        3 => -Vector2.UnitY,
                        _ when last is >= 4 and <= 7 => GetVector2(1, (last - 4) * 90 + 45),
                        _ => Vector2.Zero
                    };

                    if (moving != Vector2.Zero)
                        moving *= mission.Speed * (Slow ? 0.5f : 1);

                    mission.collidingBox.Offset(moving * 0.5f);
                    ClampHeartInBox();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsRed()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;
                    Vector2 moving = new(
                        (IsKeyDown(Keys_[0]) ? 1 : 0) - (IsKeyDown(Keys_[2]) ? 1 : 0),
                        (IsKeyDown(Keys_[1]) ? 1 : 0) - (IsKeyDown(Keys_[3]) ? 1 : 0));

                    if (moving != Vector2.Zero)
                    {
                        moving.Normalize();
                        moving *= mission.Speed * (Slow ? 0.5f : 1);
                    }

                    Vector2 finalMoving = GetVector2(moving.Length(), MathF.Atan2(moving.Y, moving.X) / PI * 180 + mission.missionRotation);

                    mission.collidingBox.Offset(finalMoving * 0.5f);
                    ClampHeartInBox();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsBlue()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;
                    float trueRot = (mission.missionRotation + 90) % 360;

                    int jumpKey = trueRot switch
                    {
                        _ when trueRot is > 45 and < 135 => 3,
                        _ when trueRot is > 135 and < 225 => 0,
                        _ when trueRot is > 115 and < 315 => 1,
                        _ => 2
                    };

                    int XWay = (IsKeyDown(Keys_[(jumpKey + 1) % 4]) ? 1 : 0) - (IsKeyDown(Keys_[(jumpKey + 3) % 4]) ? 1 : 0);

                    Vector2 oldCentre = mission.Centre;

                    float xMoved = mission.Speed * XWay * (Slow ? 0.5f : 1.0f);

                    mission.Centre += GetVector2(xMoved * 0.5f, mission.missionRotation);

                    bool res = false;
                    if (mission.collidingBox.X < _curBox.Left)
                    {
                        res = mission.YFacing == 2 && mission.collidingBox.X < _curBox.Left - 0.029f;
                        mission.collidingBox.X = _curBox.Left;
                        oldCentre.X = mission.Centre.X;
                    }
                    else if (mission.collidingBox.Right > _curBox.Right)
                    {
                        res = mission.YFacing == 0 && mission.collidingBox.Right > _curBox.CollidingBox.Right + 0.029f;
                        mission.collidingBox.X = _curBox.Right - 16;
                        oldCentre.X = mission.Centre.X;
                    }
                    if (mission.collidingBox.Y < _curBox.Up)
                    {
                        res = mission.YFacing == 3 && mission.collidingBox.Up < _curBox.CollidingBox.Up - 0.029f;
                        mission.collidingBox.Y = _curBox.Up;
                        oldCentre.Y = mission.Centre.Y;
                    }
                    else if (mission.collidingBox.Down > _curBox.Down)
                    {
                        res = mission.YFacing == 1 && mission.collidingBox.Down > _curBox.CollidingBox.Down + 0.029f;
                        mission.collidingBox.Y = _curBox.Down - 16;
                        oldCentre.Y = mission.Centre.Y;
                    }
                    if (res && mission.isForced)
                    {
                        float v = mission.forcedSpeed;
                        if (v >= 3)
                        {
                            Fight.Functions.PlaySound(FightResources.Sounds.slam, Math.Min(1, MathF.Sqrt(v - 1) / 3f));
                            InstanceCreate(new Advanced.ScreenShaker((int)Math.Ceiling(Math.Sqrt(v - 2) * 1.33f), 4 + MathF.Sqrt(v * 1.33f + 1) * 1.56f, 3, trueRot));
                        }
                        mission.isForced = false;
                    }

                    float final = 0;
                    Vector2 adapt = Vector2.Zero;

                    foreach (GravityLine v in GravityLine.GravityLines)
                    {
                        if (v.IsCollideWith(mission) && mission.gravitySpeed >= 0f)
                        {
                            final = v.Rotation / PI * 180 % 180.01f;
                            res = true;
                            adapt = v.CorrectPosition(mission);
                            break;
                        }
                    }
                    if (res)
                    {
                        mission.isForced = false;
                        if (mission.gravitySpeed >= 0)
                        {
                            mission.gravitySpeed = 0f;
                            float playerXaxisDir = mission.XFacing * 90;
                            float best = (final + 360) % 360, bestAngle = RotationDist(best, playerXaxisDir);
                            for (int i = 1; i < 4; i++)
                            {
                                float cur = final + i * 90;
                                float curAngle = RotationDist(cur, playerXaxisDir);
                                if (curAngle < bestAngle)
                                    bestAngle = curAngle;
                            }
                            mission.Centre = oldCentre + GetVector2(xMoved * 0.5f, final) + adapt;
                        }
                        mission.jumpTimeLeft = mission.JumpTimeLimit;
                    }
                    else
                    {
                        if (mission.jumpTimeLeft == mission.jumpTimeLimit && mission.gravitySpeed >= 1f)
                            mission.jumpTimeLeft = mission.jumpTimeLimit - 1;

                        mission.gravitySpeed += mission.SoftFalling ?
                            ((mission.gravitySpeed >= 0 && mission.gravitySpeed <= mission.Gravity / 5f)
                            ? MathHelper.Lerp(0.5f, 1.0f, mission.gravitySpeed / (mission.Gravity / 5f)) * mission.Gravity * 0.01f
                        : mission.Gravity / 100)
                        : mission.Gravity / 100;
                    }
                    Vector2 ori = mission.Centre;
                    mission.Centre += GetVector2(mission.gravitySpeed * 0.5f, trueRot);

                    bool jumpKeyDown = IsKeyDown(Keys_[jumpKey]) &&
                        ((!IsKeyDown(Keys_[(jumpKey + 2) % 4])) || hearts.Count >= 2);

                    if (jumpKeyDown)
                    {
                        if ((res || IsKeyPressed120f(Keys_[jumpKey])) && mission.jumpTimeLeft > 0)
                        {
                            mission.jumpTimeLeft--;
                            mission.gravitySpeed = -mission.JumpSpeed;
                        }
                    }
                    else if (mission.gravitySpeed < 0)
                    {
                        if (!mission.SoftFalling)
                        {
                            if (mission.gravitySpeed < -1.0f)
                            {
                                float next = mission.Gravity / 1.6f;
                                if (next > -1.0f)
                                    next = -0.6f;

                                mission.gravitySpeed = next;
                            }
                        }
                        else
                        {
                            if (mission.gravitySpeed < -0.4f)
                            {
                                float next = mission.Gravity / 2.4f;
                                if (next > -0.4f)
                                    next = -0.35f;

                                mission.gravitySpeed = next;
                            }
                            mission.Centre = ori + GetVector2(mission.gravitySpeed * 0.5f, trueRot);
                        }
                    }
                    if (mission.gravitySpeed > 0)
                        if (mission.umbrellaAvailable && IsKeyDown(InputIdentity.Alternate) && (!mission.isForced))
                            mission.gravitySpeed = mission.gravitySpeed * 0.85f + mission.umbrellaSpeed * 0.15f;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsGray()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;
                    float trueRot = (mission.missionRotation + 90) % 360;
                    float strength = 1.8f;

                    for (int i = 0; i < 4; i++)
                        if (IsKeyPressed120f(Keys_[i]))
                        {
                            mission.GiveForce(-90 + i * 90, strength);
                            break;
                        }

                    bool res = false;
                    if (mission.collidingBox.X < _curBox.Left)
                    {
                        res = mission.YFacing == 2 && mission.collidingBox.X < _curBox.Left - 0.9f;
                        mission.collidingBox.X = _curBox.Left;
                    }
                    else if (mission.collidingBox.Right > _curBox.Right)
                    {
                        res = mission.YFacing == 0 && mission.collidingBox.Right > _curBox.CollidingBox.Right + 0.9f;
                        mission.collidingBox.X = _curBox.Right - 16;
                    }
                    if (mission.collidingBox.Y < _curBox.Up)
                    {
                        res = mission.YFacing == 3 && mission.collidingBox.Up < _curBox.CollidingBox.Up - 0.9f;
                        mission.collidingBox.Y = _curBox.Up;
                    }
                    else if (mission.collidingBox.Down > _curBox.Down)
                    {
                        res = mission.YFacing == 1 && mission.collidingBox.Down > _curBox.CollidingBox.Down + 0.9f;
                        mission.collidingBox.Y = _curBox.Down - 16;
                    }

                    foreach (GravityLine v in GravityLine.GravityLines)
                    {
                        if (v.IsCollideWith(mission) && mission.gravitySpeed >= 0f)
                        {
                            res = true;
                            break;
                        }
                    }

                    if (res)
                    {
                        mission.gravitySpeed = 0f;
                        mission.jumpTimeLeft = mission.JumpTimeLimit;
                    }
                    else
                    {
                        if (mission.jumpTimeLeft == mission.jumpTimeLimit)
                            mission.jumpTimeLeft = mission.jumpTimeLimit - 1;

                        mission.gravitySpeed += mission.Gravity / 50f;
                    }
                    mission.Centre += GetVector2(mission.gravitySpeed, trueRot) * 0.5f;

                    if (mission.gravitySpeed > 0)
                        if (mission.umbrellaAvailable && IsKeyDown(InputIdentity.Alternate))
                            mission.gravitySpeed = mission.gravitySpeed * 0.8f + 1.0f * 0.2f;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MoveAsBlueOrange()
                {
                    RectangleBox _curBox = mission.controlingBox as RectangleBox;
                    float trueRot = (mission.missionRotation + 90) % 360;

                    int jumpKey = trueRot switch
                    {
                        _ when trueRot is > 45 and < 135 => 3,
                        _ when trueRot is > 135 and < 225 => 0,
                        _ when trueRot is > 115 and < 315 => 1,
                        _ => 2
                    };
                    int XWay = 0;
                    if (IsKeyDown(Keys_[(jumpKey + 1) % 4]))
                        blueLastWay = false;

                    if (IsKeyDown(Keys_[(jumpKey + 3) % 4]))
                        blueLastWay = true;

                    XWay -= blueLastWay ? 1 : -1;

                    float xFacing = mission.XFacing * 90;

                    Vector2 oldCentre = mission.Centre;

                    float xMoved = mission.Speed * XWay * (Slow ? 0.5f : 1);

                    mission.Centre += GetVector2(xMoved, mission.missionRotation);

                    bool res = false;
                    if (mission.collidingBox.X < _curBox.Left)
                    {
                        res = mission.YFacing == 2 && mission.collidingBox.X < _curBox.Left - 0.009f;
                        mission.collidingBox.X = _curBox.Left;
                        oldCentre.X = mission.Centre.X;
                    }
                    else if (mission.collidingBox.Right > _curBox.Right)
                    {
                        res = mission.YFacing == 0 && mission.collidingBox.Right > _curBox.CollidingBox.Right + 0.009f;
                        mission.collidingBox.X = _curBox.Right - 16;
                        oldCentre.X = mission.Centre.X;
                    }
                    if (mission.collidingBox.Y < _curBox.Up)
                    {
                        res = mission.YFacing == 3 && mission.collidingBox.Up < _curBox.CollidingBox.Up - 0.009f;
                        mission.collidingBox.Y = _curBox.Up;
                        oldCentre.Y = mission.Centre.Y;
                    }
                    else if (mission.collidingBox.Down > _curBox.Down)
                    {
                        res = mission.YFacing == 1 && mission.collidingBox.Down > _curBox.CollidingBox.Down + 0.009f;
                        mission.collidingBox.Y = _curBox.Down - 16;
                        oldCentre.Y = mission.Centre.Y;
                    }
                    if (res && mission.isForced)
                    {
                        InstanceCreate(new Advanced.ScreenShaker(2, 5, 3));
                        mission.isForced = false;
                    }

                    float final = 0;

                    foreach (GravityLine v in GravityLine.GravityLines)
                    {
                        if (v.IsCollideWith(mission) && mission.gravitySpeed >= 0f)
                        {
                            float rot = (v.Rotation / PI * 180 - xFacing + 180) % 180;
                            if (rot < -90)
                                rot += 180;

                            if (rot > 90)
                                rot -= 180;

                            final = rot;
                            res = true;
                            break;
                        }
                    }

                    if (res)
                    {
                        if (mission.gravitySpeed >= 0)
                        {
                            mission.gravitySpeed = 0f;
                            mission.Centre = oldCentre + GetVector2(xMoved, mission.missionRotation + final);
                        }
                        mission.jumpTimeLeft = mission.JumpTimeLimit;
                    }
                    else
                    {
                        if (mission.jumpTimeLeft == mission.jumpTimeLimit)
                            mission.jumpTimeLeft = mission.jumpTimeLimit - 1;

                        mission.gravitySpeed += mission.Gravity / 50f;
                    }
                    mission.Centre += GetVector2(mission.gravitySpeed, trueRot);

                    bool jumpKeyDown = !IsKeyPressed120f(Keys_[(jumpKey + 2) % 4]);

                    if (jumpKeyDown)
                    {
                        if ((res || jumpKeyDown) && mission.jumpTimeLeft > 0)
                        {
                            mission.jumpTimeLeft--;
                            mission.gravitySpeed = -mission.JumpSpeed;
                        }
                    }
                    else if (mission.gravitySpeed < 0)
                        mission.gravitySpeed /= 3f;

                    if (mission.gravitySpeed > 0)
                        if (mission.umbrellaAvailable && IsKeyDown(InputIdentity.Alternate))
                            mission.gravitySpeed = mission.gravitySpeed * 0.8f + mission.umbrellaSpeed * 0.2f;
                }
            }
        }
    }
}