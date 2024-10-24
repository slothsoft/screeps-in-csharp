using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public struct BodyPartGroup {

    public static BodyPartGroup Variable(int minCount = 1, int maxCount = 1, params BodyPartType[] types) {
        return new BodyPartGroup(types) {
            MaxCount = 3,
        };
    }
    
    public static BodyPartGroup Fixed(int count = 1, params BodyPartType[] types) {
        return new BodyPartGroup(types) {
            MinCount = count,
            MaxCount = count,
        };
    }

    private readonly BodyPartType[] _types;
    
    private BodyPartGroup(params BodyPartType[] types) {
        _types = types;
    }

    public int MinCount { get; set; } = 1;
    public int MaxCount { get; set; } = 1;
    public IReadOnlyCollection<BodyPartType> BodyPartTypes => _types;


}