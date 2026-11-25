namespace Marple.Core {
    /// <summary>
    /// Something whose changes can be detected by polling. The id goes up whenever the
    /// observed thing changes; observers store the last id they saw and compare.
    /// </summary>
    public interface IPassiveObservable {
        public int GetObservableId();
    }
}
