using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A undertaker will take energy from tombstones or ruins or piles lying around.
/// </summary>
public class Undertaker : IJob {
    internal const string JobId = "undertaker";
    private static readonly BodyPartGroup[] UndertakerBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 5, BodyPartType.Move), 
        BodyPartGroup.Fixed(1, BodyPartType.Carry, BodyPartType.Work) 
    ];
    
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;

    public Undertaker(RoomCache room, CreepManager creepManager) {
        _room = room;
        _creepManager = creepManager;
    }

    public string Id => JobId;
    public string Icon => "\u26b0\ufe0f";
    public int WantedCreepCount => _creepManager.GetAllCreeps().Count() >= 9 && !_creepManager.GetCreeps(Guard.JobId).Any() ? 1 : 0; // TODO magic number
    public IEnumerable<BodyPartGroup> BodyPartGroups => UndertakerBodyPartGroups;
    public int Priority => 200;

    public void Run(ICreep creep) {
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            creep.MoveToPickupLostResources(_room);
        } else {
            creep.MoveToTransferIntoStorage(_room);
        }
    }
}