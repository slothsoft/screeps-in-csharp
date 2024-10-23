using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Creeps are assigned a job at birth and have to do it
/// until they die.
/// </summary>
public interface IJob {
    protected static readonly BodyPartGroup[] DefaultBodyPartGroups = [ BodyPartGroup.Variable(BodyPartType.Carry, BodyPartType.Work), BodyPartGroup.Fixed(BodyPartType.Move) ];
    
    string Id { get; }
    string Icon { get; }
    int WantedCreepCount { get; }
    IEnumerable<BodyPartGroup> BodyPartGroups { get; }
    void Run(ICreep creep);
}