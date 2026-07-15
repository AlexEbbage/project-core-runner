using CoreRacer.Meta.Profile;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class ProfileMigrationServiceTests
    {
        [Test]
        public void MigrateNullProfileReturnsUsableDefaultProfile()
        {
            var migrated = new ProfileMigrationService().Migrate(null);

            Assert.NotNull(migrated);
            Assert.AreEqual(3, migrated.Version);
            Assert.AreEqual(1, migrated.Level);
            Assert.AreEqual("starter_runner", migrated.SelectedShipId);
            Assert.IsTrue(migrated.Inventory.IsUnlocked("starter_runner"));
            Assert.IsTrue(migrated.Inventory.IsUnlocked("classic_white"));
            Assert.IsTrue(migrated.Inventory.IsUnlocked("pulse_wake"));
            Assert.IsTrue(migrated.Inventory.IsUnlocked("starter_glow"));
            Assert.NotNull(migrated.EquippedBoosterIds);
        }
    }
}
