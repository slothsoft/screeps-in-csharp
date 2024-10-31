using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public class StructureManager : IManager {
    private readonly IGame _game;
    private readonly RoomCache _room;

    private readonly IDictionary<StructureType, IPurpose> _structurePurposes;
    
    public StructureManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;

        _structurePurposes = new Dictionary<StructureType, IPurpose> {
            {StructureType.Tower, new TowerPurpose()}
        };
    }

    public void Tick() {
        foreach (var structure in _room.AllStructures) {
            foreach (var typePurpose in _structurePurposes) {
                if (typePurpose.Key.IsAssignableFrom(structure)) {
                    typePurpose.Value.Run(structure);
                }
            }
        }
    }
}