using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A scout runs around, finds room information and kills enemies.
/// </summary>
public class Scout : IJob {
    private const string JobId = "scout";
    private static readonly BodyPartGroup[] ScoutBodyPartGroups = [ 
        BodyPartGroup.Variable(0, 5, BodyPartType.Tough, BodyPartType.Move), 
        BodyPartGroup.Variable(1, 5, BodyPartType.Move, BodyPartType.Attack), 
    ];
    private const string HistorySeparator = " > ";

    private readonly IGame _game;
    private readonly RoomCache _room;
    
    private readonly ExitDirection[] _existDirections = Enum.GetValues<ExitDirection>().ToArray();

    public Scout(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => JobId;
    public string Icon => "\ud83c\udfa7";
    public int WantedCreepCount => 1;
    public IEnumerable<BodyPartGroup> BodyPartGroups => ScoutBodyPartGroups;
    public int Priority => 90;

    public void Run(ICreep creep) {
        var room = creep.Room!; // not that _room is the spawn room, but creep.Room is the current one
        
        var reconnaissanceData = _game.GetReconnaissanceData(room.Coord);
        var directionToPoints = _existDirections.ToDictionary(dir => dir, dir => room.FindExits(dir).ToArray());
        var exitDescription = _game.Map.DescribeExits(room.Coord);
        
        // 0th priority make sure the current room is in the history
        creep.Memory.TryGetString(CreepHistory, out var roomCoordHistory);
        roomCoordHistory ??= string.Empty;
        if (!roomCoordHistory.EndsWith(room.Coord.ToString())) {
            roomCoordHistory = roomCoordHistory.Length == 0 ? room.Coord.ToString() : (roomCoordHistory + HistorySeparator + room.Coord);
            creep.Memory.SetValue(CreepHistory, roomCoordHistory);
            
            // 1st priority: scan room if necessary
            reconnaissanceData.SetValue(ReconnaissanceDataSourceCount, room.Find<ISource>().Count());
            reconnaissanceData.SetValue(ReconnaissanceDataMineralCount, room.Find<IMineral>().Count());
            reconnaissanceData.SetValue(ReconnaissanceDataEnemyStructureCount, room.Find<IStructure>(false).Count());
        
            foreach (var existDirection in _existDirections) {
                if (directionToPoints[existDirection].Length > 0) {
                    reconnaissanceData.SetValue(ReconnaissanceDataExitTiles + existDirection, directionToPoints[existDirection].Length);
                }
            }
        }

        // 2nd priority: suicide
        if (!creep.Body.Any(p => p.Type is BodyPartType.Attack or BodyPartType.RangedAttack)) {
            // we can't attack any longer, so suicide
            creep.Memory.SetValue(CreepSuicide, false);
        }
        if (creep.MoveToRecycleAtSpawnIfNecessary(_room)) {
            return;
        }

        // 3rd priority: if we already have a target, follow it, else try to attack enemies
        if (creep.MoveToAttackInSameRoom()) {
            return;
        }

        // 4th priority: move to the next best exit
        var availableExits = directionToPoints
            .Where(kv => kv.Value.Length > 0)
            // all points for one exit either all have the same x or all the same y. So min and max of both will be the most left / top most point and the most right /bottom most
            .ToDictionary(kv => kv.Key, kv => (Min:kv.Value.MinBy(v => v.X + v.Y), Max:kv.Value.MaxBy(v => v.X + v.Y)))
            .Distinct().ToArray();
        
        var bestExitPoint = availableExits
            // only take into account rooms we have no data of for now
            .Where(dirPoint => !reconnaissanceData.TryGetObject(exitDescription[dirPoint.Key]!.Value.ToString(), out _))
            .SelectMany(kv => new [] { kv.Value.Min, kv.Value.Max })
            // now calculate the distance to the current creep and take the shortest
            .MinBy(point => creep.LocalPosition.LinearDistanceTo(point));

        if (bestExitPoint != default) {
        } 
        
        // 5th priority: we already know the neighboring rooms, so try to prevent rooms in the history
        var minIndex = _existDirections
            .Where(d => exitDescription[d].HasValue)
            .Min(d => roomCoordHistory.LastIndexOf(exitDescription[d]!.Value.ToString(), StringComparison.Ordinal));
        var pointWithEarliestHistory = availableExits
            .Where(kv => minIndex == roomCoordHistory.LastIndexOf(exitDescription[kv.Key]!.Value.ToString(), StringComparison.Ordinal))
            .SelectMany(kv => new[] { kv.Value.Min, kv.Value.Max })
            .MinBy(point => creep.LocalPosition.LinearDistanceTo(point));
                
        creep.MoveTo(pointWithEarliestHistory);
    }
}