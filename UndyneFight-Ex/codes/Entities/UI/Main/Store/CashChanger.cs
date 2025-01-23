namespace UndyneFight_Ex.Entities
{
    internal partial class StoreFront
    {
        internal class CashChanger(CollideRect area) : StoreArea(area)
        {
            internal class CashChangerUI : Selector
            {

            }
            protected override Selector AreaEntity => new CashChangerUI();

            public override void Update()
            {
            }
        }
    }
}