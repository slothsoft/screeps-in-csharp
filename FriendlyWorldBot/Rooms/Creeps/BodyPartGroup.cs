using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public struct BodyPartGroup {

    public static BodyPartGroup Variable(params BodyPartType[] types) {
        return new BodyPartGroup(types) {
            IsVariable = true,
            MaxCount = 3,
        };
    }
    
    public static BodyPartGroup Fixed(params BodyPartType[] types) {
        return new BodyPartGroup(types) {
            IsVariable = false,
            MaxCount = 1,
        };
    }

    private readonly BodyPartType[] _types;
    
    private BodyPartGroup(params BodyPartType[] types) {
        _types = types;
    }

    public bool IsVariable { get; set; } = false;
    public int MinCount { get; set; } = 1;
    public int MaxCount { get; set; } = 3;
    public IReadOnlyCollection<BodyPartType> BodyPartTypes => _types;


}