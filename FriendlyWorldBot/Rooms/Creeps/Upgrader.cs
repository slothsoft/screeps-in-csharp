using System.Collections.Generic;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// The upgrader job will instruct creeps to collect energy from the spawn, then upgrade the room controller.
/// Spawns will be cached to the heap for efficiency.
/// </summary>
public class Upgrader : IJob
{
    private readonly RoomCache _room;
    
    public Upgrader(RoomCache room) {
        _room = room;
    }
    
    public string Id { get; } = "upgrader";
    public string Icon { get; } = "🗼";
    public int WantedCreepCount => 3;
    public IEnumerable<BodyPartGroup> BodyPartGroups => IJob.DefaultBodyPartGroups;
    
    public void Run(ICreep creep)
    {
        // Check energy storage
        if (creep.Store[ResourceType.Energy] > 0) {
            // There is energy to use - upgrade the controller
            var controller = _room.Room.Controller;
            if (controller == null) {
                return;
            }
            var upgradeResult = creep.UpgradeController(controller);
            if (upgradeResult == CreepUpgradeControllerResult.NotInRange) {
                creep.BetterMoveTo(controller.RoomPosition);
            } else if (upgradeResult != CreepUpgradeControllerResult.Ok) {
                creep.Say("⚠");
                creep.LogInfo($"unexpected result when depositing to {controller} ({upgradeResult})");
            }
        } else {
            // We're empty - go to pick up
            var spawn = _room.FindNearestSpawn(creep.LocalPosition);
            if (spawn == null || spawn.Store.GetUsedCapacity() < 10) { // TODO: magic number
                creep.MoveToHarvestInRoom(_room);
                return; 
            }
            creep.MoveToWithdraw(spawn);
        }
    }
}