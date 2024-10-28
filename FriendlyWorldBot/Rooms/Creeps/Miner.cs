using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// The miner job has to be placed in front of a source on a container, and puts energy into the storage.
/// </summary>
public class Miner : IJob {
    private static readonly BodyPartGroup[] MinerBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 1, BodyPartType.Work), 
        BodyPartGroup.Fixed(1, BodyPartType.Move), 
    ];
    
    private readonly RoomCache _room;
    private readonly ICreepsCache _creeps;

    public Miner(RoomCache room, ICreepsCache creeps) {
        _room = room;
        _creeps = creeps;
    }

    public string Id => "miner";
    public string Icon => "\u26cf\ufe0f";
    public int WantedCreepCount => _room.Room.Find<IStructureContainer>()
        .Select(c => c.GetMemory().TryGetString(ContainerWithSource, out var source) ? source : null)
        .Where(t => !string.IsNullOrWhiteSpace(t))
        .OfType<string>().Count();
    public IEnumerable<BodyPartGroup> BodyPartGroups => MinerBodyPartGroups;
    public int Priority => 0;
    
    public void Run(ICreep creep) {
        if (creep.MoveToRecycleAtSpawnIfNecessary(_room)) {
            return;
        }
        
        if (!creep.Memory.TryGetString(CreepTarget, out var targetId) || string.IsNullOrWhiteSpace(targetId)) {
            var usedContainers =_creeps.GetCreeps(Id)
                .Select(c => c.Memory.TryGetString(CreepTarget, out var target) ? target : null)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .OfType<string>().ToArray();
            var miningContainerIds = _room.Room.Find<IStructureContainer>()
                .Where(c => c.GetMemory().TryGetString(ContainerWithSource, out var source) && !string.IsNullOrWhiteSpace(source))
                .Select(t => t.Id.ToString()).ToList();
            miningContainerIds.RemoveRange(usedContainers);
            targetId = miningContainerIds.FirstOrDefault() ?? usedContainers.FirstOrDefault();
        }
        var target = _room.Room.Find<IStructureContainer>().SingleOrDefault(c => c.Id == targetId);
        if (target == null) {
            creep.LogError($"Could not find mining container in room {_room.Room.Name}");
            return; // we could not find a container
        }
        creep.Memory.SetValue(CreepTarget, targetId!);

        if (target.RoomPosition != creep.RoomPosition) {
            creep.BetterMoveTo(target.RoomPosition);
            return;
        }

        var source = target.GetMemory().TryGetString(ContainerWithSource, out var sourceId) ? _room.Sources.Single(s => s.Id == sourceId) : null;
        if (source == null) {
            creep.LogError($"Could not find source for container {target.Id} in room {_room.Room.Name}");
            return;
        }
        
        // we are at the container. now mine!
        creep.MoveToHarvest(source);
    }
}