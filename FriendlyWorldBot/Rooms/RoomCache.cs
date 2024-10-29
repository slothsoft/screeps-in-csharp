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
    public IList<IStructureSpawn> Spawns { get; } = new List<IStructureSpawn>();
    public IList<IStructureExtension> Extensions { get; } = new List<IStructureExtension>();
    public IList<IStructureRampart> Ramparts { get; } = new List<IStructureRampart>();
    public IList<IStructureTower> Towers { get; } = new List<IStructureTower>();
    public IList<IStructure> OtherStructures { get; } = new List<IStructure>();
    public IList<IStructure> AllStructures { get; } = new List<IStructure>();
    
    public IList<ISource> Sources {
        get {
            _sources ??= Room.Find<ISource>().ToList();
            return _sources;
        }
    }
    
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
                switch (structure) {
                    case IStructureSpawn spawn:
                        Spawns.Add(spawn);
                        break;
                    case IStructureExtension extension:
                        Extensions.Add(extension);
                        break;
                    case IStructureRampart rampart:
                        Ramparts.Add(rampart);
                        break;
                    case IStructureTower tower:
                        Towers.Add(tower);
                        break;
                    default:
                        OtherStructures.Add(structure);
                        break;
                }
                AllStructures.Add(structure);
            }
        }
    }
}