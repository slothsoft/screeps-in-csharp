using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

public class RoomCache {
    private IRoom _room;
    private IList<ISource>? _sources;
    private IList<IStructureSpawn>? _spawns;
    private IList<IStructureExtension>? _extensions;
    private IList<IStructureRampart>? _ramparts;
    private string? _name;
    private string? _shortName;

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

    public IEnumerable<IStructureSpawn> SpawnsForExtensionConstruction {
        get {
            yield return MainSpawn;
        }
    }

    public IStructureSpawn MainSpawn {
        get {
            var mainSpawn = Spawns.SingleOrDefault(s => s.Memory.TryGetBool(SpawnMain, out var spawnMain) && spawnMain);
            if (mainSpawn == null) {
                mainSpawn = Spawns.First();
                foreach (var spawn in Spawns) {
                    spawn.Memory.SetValue(SpawnMain, spawn == mainSpawn);
                }
            }

            return mainSpawn;
        }
    }
    
    public IList<IStructureExtension> Extensions {
        get {
            if (_extensions == null) {
                _extensions = new List<IStructureExtension>();
                foreach (var extension in _room.Find<IStructureExtension>()) {
                    _extensions.Add(extension);
                }
            }

            return _extensions;
        }
    }

    public IList<IStructureRampart> Ramparts {
        get {
            if (_ramparts == null) {
                _ramparts = new List<IStructureRampart>();
                foreach (var rampart in _room.Find<IStructureRampart>()) {
                    _ramparts.Add(rampart);
                }
            }

            return _ramparts;
        }
    }
    
    public IRoom Room => _room;


    public string Name {
        get {
            if (_name == null) {
                // try to take the name out of the room memories
                if (_room.Memory.TryGetString(RoomName, out var name)) {
                    _name = name;
                }

                // if the name is not yet set, take the first spawn's name
                if (string.IsNullOrWhiteSpace(_name)) {
                    _name = Spawns.FirstOrDefault()?.Name ?? string.Empty;
                }

                _room.Memory.SetValue(RoomName, _name);
                // if there is no name, take the room's ID
                if (string.IsNullOrWhiteSpace(_name)) {
                    return _room.Name;
                }
            }

            return _name;
        }
    }

    public string ShortName {
        get {
            if (_shortName == null) {
                // try to take the short name out of the room memories
                if (_room.Memory.TryGetString(RoomNameShort, out var shortName)) {
                    _shortName = shortName;
                }

                // if the name is not yet set, take the first letter of the name
                if (string.IsNullOrWhiteSpace(_shortName)) {
                    _shortName = Name[0..1];
                }

                _room.Memory.SetValue(RoomNameShort, _shortName);
            }

            return _shortName;
        }
    }

    public ISource? FindNearestSource(Position pos) {
        return Sources.FindNearest(pos);
    }

    public IStructureSpawn? FindNearestSpawn(Position pos) {
        return Spawns.FindNearest(pos);
    }
}