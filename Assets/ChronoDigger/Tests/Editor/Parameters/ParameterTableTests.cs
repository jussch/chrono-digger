using System;
using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ParameterTableTests {
        private readonly object _buff = new object();

        [Test]
        public void Get_NotInProfile_ReturnsDefault() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = new ParameterTable(TestStats.ParameterProfile());

            Assert.AreEqual((Amount)4, block.Get(speed));
        }

        [Test]
        public void Get_InProfile_ReturnsProfileValue() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = new ParameterTable(TestStats.ParameterProfile((speed, 9)));

            Assert.AreEqual((Amount)9, block.Get(speed));
        }

        [Test]
        public void GetAmount_AddModifier_ChangesGet() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = new ParameterTable(TestStats.ParameterProfile());

            block.GetAmount(speed).Add(_buff, AmountModifier.Raw(2));

            Assert.AreEqual((Amount)6, block.Get(speed));
        }

        [Test]
        public void Constructor_NullParameter_Throws() {
            Assert.Throws<ArgumentException>(() => new ParameterTable(TestStats.ParameterProfile((null, 1))));
        }

        [Test]
        public void Constructor_DuplicateParameter_Throws() {
            var speed = TestStats.Parameter("Speed");
            Assert.Throws<ArgumentException>(() => new ParameterTable(TestStats.ParameterProfile((speed, 1), (speed, 2))));
        }
    }
}
