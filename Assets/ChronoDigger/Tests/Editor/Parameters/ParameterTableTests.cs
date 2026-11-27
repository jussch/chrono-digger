using System;
using System.Collections.Generic;
using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ParameterTableTests {
        private readonly object _buff = new object();

        private static ParameterTable Table(ParameterProfile profile) => new ParameterTable(profile, new StatTable(TestStats.Profile()));

        [Test]
        public void GetValue_NotInProfile_ReturnsDefault() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = Table(TestStats.ParameterProfile());

            Assert.AreEqual((Amount)4, block.GetValue(speed));
        }

        [Test]
        public void GetValue_InProfile_ReturnsProfileValue() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = Table(TestStats.ParameterProfile((speed, 9)));

            Assert.AreEqual((Amount)9, block.GetValue(speed));
        }

        [Test]
        public void Get_AddModifier_ChangesValue() {
            var speed = TestStats.Parameter("Speed", 4);
            var block = Table(TestStats.ParameterProfile());

            block.Get(speed).Add(_buff, AmountModifier.Raw(2));

            Assert.AreEqual((Amount)6, block.GetValue(speed));
        }

        [Test]
        public void Constructor_NullParameter_Throws() {
            Assert.Throws<ArgumentException>(() => Table(TestStats.ParameterProfile((null, 1))));
        }

        [Test]
        public void Constructor_DuplicateParameter_Throws() {
            var speed = TestStats.Parameter("Speed");
            Assert.Throws<ArgumentException>(() => Table(TestStats.ParameterProfile((speed, 1), (speed, 2))));
        }

        [Test]
        public void Constructor_StatModifier_MultiplierReadsStat() {
            var speed = TestStats.Parameter("Speed");
            var agility = TestStats.Create("Agility");
            var profile = TestStats.ParameterProfile((speed, 4));
            var entry = profile.Parameters[0];
            entry.StatModifiers = new List<StatModifier> { new StatModifier { Stat = agility, Modifier = AmountModifier.Raw(2) } };
            profile.Parameters[0] = entry;
            var stats = new StatTable(TestStats.Profile((agility, 3)));

            var block = new ParameterTable(profile, stats);

            Assert.AreEqual((Amount)10, block.GetValue(speed));
        }
    }
}
