using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public interface ICreepsCache {
    ISet<ICreep> GetCreeps(string jobId);
    IEnumerable<ICreep> GetAllCreeps();
}