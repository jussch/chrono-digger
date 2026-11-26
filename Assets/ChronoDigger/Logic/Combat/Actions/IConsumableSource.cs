namespace ChronoDigger.Combat {
    public interface IConsumableSource : IActionSource {
        bool CanConsume(int amount);
        void Consume(int amount);
    }
}
