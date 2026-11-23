using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests
{
    public class PlaceholderTests
    {
        [Test]
        public void Add_Works() => Assert.AreEqual(4, Placeholder.Add(2, 2));
    }
}
