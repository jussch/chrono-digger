using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests
{
    public class AmountTests
    {
        [Test]
        public void Add_Works() => Assert.AreEqual(4, (Amount.Wrap(1) + Amount.Wrap(3)).FloorToIntClamped());
    }
}
