namespace UndyneFight_Ex.Entities
{
    public partial class Player
    {
        /// <summary>
        /// The state of heart movement
        /// </summary>
        /// <param name="color">The color of the heart</param>
        /// <param name="moveFunction">The function of the heart movement</param>
        public class MoveState(col color, Action<Heart> moveFunction)
        {
            /// <summary>
            /// The color of the heart
            /// </summary>
            public col StateColor { get; init; } = color;
            /// <summary>
            /// The function of the heart movement
            /// </summary>
            public Action<Heart> MoveFunction { get; init; } = moveFunction;
        }
        public partial class Heart
        {
            /// <summary>
            /// The current move state of the heart
            /// </summary>
            public MoveState CurrentMoveState { get; private set; }

            private static readonly MoveState _red = new(col.Red, (s) =>
            {
                Move.mission = s;
                if (s.isOranged)
                    Move.MoveAsOrange();
                else
                    Move.MoveAsRed();
            });
            private static readonly MoveState _green = new(col.Lime, (s) => Move.mission = s);
            private static readonly MoveState _blue = new(col.Blue, (s) =>
            {
                Move.mission = s;
                if (s.isOranged)
                    Move.MoveAsBlueOrange();
                else
                    Move.MoveAsBlue();
            });
            private static readonly MoveState _purple = new(col.MediumPurple, (s) =>
            {
                Move.mission = s;
                Move.MoveAsPurple();
            });
            private static readonly MoveState _gray = new(col.Gray, (s) =>
            {
                Move.mission = s;
                Move.MoveAsGray();
            });
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ChangeState(MoveState state)
            {
                isOranged = false;
                CurrentMoveState = state;
                lastChangeTime = 0;
                SoulType = -1;
                Player manager = FatherObject as Player;
                manager.GameAnalyzer.PushData(new SoulChangeData(-1, ID, Fight.Functions.GametimeF));
                _ = CreateShinyEffect(CurrentMoveState.StateColor);
            }
        }
    }
}