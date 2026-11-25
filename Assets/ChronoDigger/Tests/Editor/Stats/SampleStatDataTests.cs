using ChronoDigger.Combat.Stats;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests.Stats
{
    public class SampleStatDataTests
    {
        [TearDown]
        public void ResetCoreStats() => CoreStats.SetInstance(null);

        [Test]
        public void GoblinShaman_GetsManaFromWizardry()
        {
            var block = new StatBlock(SampleStatData.GoblinShaman);
            block.Calculate();

            Amount expected = SampleStatData.MaxMana.defaultValue + 3 * SampleStatData.ManaPerWizardry;
            Assert.AreEqual(expected, block.Get(Stat.MaxMana));
        }

        [Test]
        public void Stat_DefaultsToSampleDefinitions()
        {
            Assert.AreSame(SampleStatData.Wizardry, Stat.Wizardry);
            Assert.AreSame(SampleStatData.MaxHealth, Stat.MaxHealth);
        }

        [Test]
        public void Stat_FollowsSwappedInstance()
        {
            var custom = TestStats.Create("Custom Health");
            CoreStats.SetInstance(new CoreStats { MaxHealth = custom });

            Assert.AreSame(custom, Stat.MaxHealth);
        }

        [Test]
        public void Fire_IsResistedByFireResistance()
        {
            Assert.AreSame(SampleStatData.FireResistance, SampleStatData.Fire.resistanceStat);
        }
    }
}
