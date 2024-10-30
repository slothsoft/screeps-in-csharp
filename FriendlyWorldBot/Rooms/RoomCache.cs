using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

public class RoomCache : IManager {
    private IList<ISource>? _sources;
    private string? _name;
    private string? _shortName;

    public RoomCache(IRoom room) {
        Room = room;
    }

    public IRoom Room { get; init; }
    public IList<ISource> Sources {
        get {
            _sources ??= Room.Find<ISource>().ToList();
            return _sources;
        }
    }
    public IEnumerable<IStructureSpawn> Spawns => AllStructures.OfType<IStructureSpawn>();
    public IEnumerable<IStructureExtension> Extensions => AllStructures.OfType<IStructureExtension>();
    public IEnumerable<IStructureRampart> Ramparts => AllStructures.OfType<IStructureRampart>();
    public IEnumerable<IStructureTower> Towers => AllStructures.OfType<IStructureTower>();
    public IList<IStructure> AllStructures { get; } = new List<IStructure>();
    
    public IEnumerable<IStructureSpawn> SpawnsForExtensionConstruction {
        get {
            var mainSpawn = MainSpawn;
            if (mainSpawn != null) {
                yield return mainSpawn;
            }
        }
    }

    public IStructureSpawn? MainSpawn {
        get {
            var mainSpawn = Spawns.SingleOrDefault(s => s.Memory.TryGetBool(SpawnMain, out var spawnMain) && spawnMain);
            if (mainSpawn == null) {
                mainSpawn = Spawns.FirstOrDefault();
                foreach (var spawn in Spawns) {
                    spawn.Memory.SetValue(SpawnMain, spawn == mainSpawn);
                }
            }
            return mainSpawn;
        }
    }
    
    public string Name {
        get {
            if (_name == null) {
                // try to take the name out of the room memories
                if (Room.Memory.TryGetString(RoomName, out var name)) {
                    _name = name;
                }

                // if the name is not yet set, take the first spawn's name
                if (string.IsNullOrWhiteSpace(_name)) {
                    _name = Spawns.FirstOrDefault()?.Name ?? string.Empty;
                }

                Room.Memory.SetValue(RoomName, _name);
                // if there is no name, take the room's ID
                if (string.IsNullOrWhiteSpace(_name)) {
                    return Room.Name;
                }
            }

            return _name;
        }
    }

    public string ShortName {
        get {
            if (_shortName == null) {
                // try to take the short name out of the room memories
                if (Room.Memory.TryGetString(RoomNameShort, out var shortName)) {
                    _shortName = shortName;
                }

                // if the name is not yet set, take the first letter of the name
                if (string.IsNullOrWhiteSpace(_shortName)) {
                    _shortName = Name[0..1];
                }

                Room.Memory.SetValue(RoomNameShort, _shortName);
            }

            return _shortName;
        }
    }

    public void Tick() {
        var structures = Room.Find<IStructure>();
        foreach (var structure in structures) {
            if (!AllStructures.Contains(structure)) {
                AllStructures.Add(structure);
            }
        }
        
        foreach (var structure in AllStructures.ToArray()) {
            if (!structure.Exists) {
                AllStructures.Remove(structure);
            }
        }
    }
}