using ChronoDigger.Combat;

namespace ChronoDigger.Tests {
    internal class FakeCost : ActionCost {
        public bool Payable = true;
        public int TimesPaid;

        public override bool CanPay(ActionContext context) => Payable;
        public override void Pay(ActionContext context) => TimesPaid++;
    }
}
