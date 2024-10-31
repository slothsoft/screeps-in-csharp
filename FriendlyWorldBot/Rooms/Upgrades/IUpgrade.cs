namespace FriendlyWorldBot.Rooms.Upgrades;

public interface IUpgrade {
    string Id { get; }
    bool ShouldBeStarted();
    UpgradeStatus Run();
}