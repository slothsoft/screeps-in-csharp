using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms;

public class RoomCache {
    private IRoom _room;
    private IList<ISource>? _sources;
    private IList<IStructureSpawn>? _spawns;

    public RoomCache(IRoom room) {
        _room = room;
    }

    public IList<ISource> Sources {
        get {
            if (_sources == null) {
                _sources = new List<ISource>();
                foreach (var source in _room.Find<ISource>()) {
                    _sources.Add(source);
                }
            }

            return _sources;
        }
    }

    public IList<IStructureSpawn> Spawns {
        get {
            if (_spawns == null) {
                _spawns = new List<IStructureSpawn>();
                foreach (var spawn in _room.Find<IStructureSpawn>()) {
                    _spawns.Add(spawn);
                }
            }

            return _spawns;
        }
    }

    public IRoom Room => _room;

    public ISource? FindNearestSource(Position pos) {
        return Sources.FindNearest(pos);
    }

    public IStructureSpawn? FindNearestSpawn(Position pos) {
        return Spawns.FindNearest(pos);
    }
}