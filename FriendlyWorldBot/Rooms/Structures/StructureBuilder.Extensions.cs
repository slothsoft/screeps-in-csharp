using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

public partial class StructureBuilder
{
    private bool BuildExtensions() {
        var controller = _room.Room.Controller;
        if (controller == null) return false; 
        var possibleExtensions = GetPossibleExtensionCount(controller.Level);
        var existingExtensions = _room.Extensions.Count;
        var showExtensions = _game.GetConfigBool("showExtensions");

        var somethingWasBuild = false;
        if (possibleExtensions > existingExtensions || showExtensions) {
            var additionalExtensions = _room.Room.Memory.TryGetInt(RoomAdditionalExtensions, out var ae) ? ae : 0;
            var positions = CreateExtensionPositions(_room.SpawnsForExtensionConstruction.ToArray(), out var skipped, possibleExtensions + additionalExtensions).ToList();

            if (possibleExtensions > existingExtensions) {
                var minX = positions.Select(p => p.X).Min();
                var minY = positions.Select(p => p.Y).Min();
                var maxX = positions.Select(p => p.X).Max();
                var maxY = positions.Select(p => p.Y).Max();

                // find out if we can place extensions at the points in question or if we need to get additional ones
                var newAdditionalExtensions = skipped[0];
                var area = _room.Room.LookAtArea(new Position(minX, minY), new Position(maxX, maxY)).ToList();
                foreach (var position in positions) {
                    var stuffAtPosition = area.Where(o => o.LocalPosition == position).ToArray();
                    var isExtension = stuffAtPosition.Any(s => s is IStructureExtension);
                    var isExtensionInConstruction = stuffAtPosition.Any(s => s is IConstructionSite cs && cs.IsStructure<IStructureExtension>());
                    
                    if (isExtension || isExtensionInConstruction) {
                        continue;
                    } 
                    if (stuffAtPosition.Length > 0) {
                        // there is another object on this position
                        newAdditionalExtensions++;
                        continue;
                    }
                    // if the place is empty, just build
                    if (_room.Room.CreateConstructionSite<IStructureExtension>(position) == RoomCreateConstructionSiteResult.Ok)
                    {
                        somethingWasBuild = true;
                    }
                }
                
                _room.Room.Memory.SetValue(RoomAdditionalExtensions, newAdditionalExtensions);
            }
            
            // visualize if necessary
            if (showExtensions) {
                foreach (var fractionalPosition in positions) {
                    _room.Room.Visual.Circle(fractionalPosition, new CircleVisualStyle(
                        Radius: 0.5,
                        Stroke: Color.White,
                        StrokeWidth: 0.05));
                }
            }
        }

        return somethingWasBuild;
    }

    private int GetPossibleExtensionCount(int roomLevel) {
        return _game.Constants.Controller.GetMaxStructureCount<IStructureExtension>(roomLevel);
    }

    public static IEnumerable<Position> CreateExtensionPositions(ICollection<IStructureSpawn> spawns, out int[] skipped, int expectedCount = 10) {
        IEnumerable<Position> result = new List<Position>();
        if (spawns.Count == 0) {
            skipped = [0];
            return result;
        }
        
        var expectedPerSpawn = expectedCount / spawns.Count;
        int[] array = [0];
        foreach (var spawn in spawns) {
            result = result
                .Concat(expectedPerSpawn.ToUlamSpiral(p => {
                            var r = IsValidExtensionPosition(p);
                            if (!r) {
                                array[0]++;
                            }
                            return r;
                        }).Select(pos => new Position(
                            pos.X + spawn.LocalPosition.X,
                            pos.Y + spawn.LocalPosition.Y
                        )));
        }
        skipped = array;
        return result;
    }

    public static bool IsValidExtensionPosition(Position pos) => IsValidExtensionPosition(pos.X, pos.Y);

    private static bool IsValidExtensionPosition(int x, int y) {
        var absX = Math.Abs(x);
        var absY = Math.Abs(y);
        if (absX < 2 && absY < 2) {
            // too close to the spawn
            return false;
        }

        if (absX == absY) {
            // these are the diagonal lines from the spawn
            return false;
        }

        if (absY < 2) {
            // the wide horizontal road
            return false;
        }

        if (absX == 0) {
            // the vertical road
            return false;
        }

        return true;
    }
}