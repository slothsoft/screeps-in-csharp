using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Creeps are assigned a job at birth and have to do it
/// until they die.
/// </summary>
public interface IJob {
    protected static readonly BodyPartGroup[] DefaultBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 3, BodyPartType.Carry, BodyPartType.Work), 
        BodyPartGroup.Fixed(1, BodyPartType.Move) 
    ];
    
    string Id { get; }
    string Icon { get; }
    int WantedCreepCount { get; }
    IEnumerable<BodyPartGroup> BodyPartGroups { get; }
    void Run(ICreep creep);

    public void OnCreepSpawned(ICreep creep)
    {
        // nothing to do on default
    }

    public void OnCreepDied(ICreep creep)
    {
        // nothing to do on default
    }
}