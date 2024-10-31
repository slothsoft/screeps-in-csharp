using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public class TowerPurpose : BasePurpose<IStructureTower> {
    protected override void RunForStructure(IStructureTower tower) => tower.AttackNearestEnemy();
}