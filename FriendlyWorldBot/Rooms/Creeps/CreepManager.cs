using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public class CreepManager : IManager {

    internal static PolyVisualStyle? PathStyle; 
    private static readonly Random Random = new();
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly IDictionary<string, IJob> _jobs;
    private readonly IDictionary<string, ISet<ICreep>> _creeps;

    public CreepManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;
        
        _jobs = CreateJobMap(game, room);
        _creeps = CreateCreepMap(_jobs.Keys);
    }

    // Populate job map - the job instances will live in the heap until the next IVM reset
    private static IDictionary<string, IJob> CreateJobMap(IGame game, RoomCache room) {
        var result = new List<IJob> {
            new Miner(room),
            new Upgrader(room),
            new Builder(game, room)
        };
        return result.ToDictionary(j => j.Id, j => j);
    }

    private static IDictionary<string, ISet<ICreep>> CreateCreepMap(IEnumerable<string> jobIds) {
        return jobIds.ToDictionary(j => j, _ => (ISet<ICreep>) new HashSet<ICreep>());
    }

    public IEnumerable<string> JobIds => _creeps.Keys;

    public int GetActualCreepCount(string jobId) {
        return _creeps[jobId].Count;
    }
    
    public IJob GetJob(string jobId) {
        return _jobs[jobId];
    }

    public void Tick() {
        var showPaths = _game.Memory.GetConfigBool("showPaths");
        PathStyle = showPaths ? new PolyVisualStyle(Stroke: Color.White) : null;
            
        // Check for any creeps we're tracking that no longer exist
        foreach (var creepList in _creeps.Values) {
            foreach (var creep in creepList.ToImmutableArray()) {
                if (creep.Exists) {
                    continue;
                }

                creepList.Remove(creep);
                OnCreepDied(creep);
            }
        }

        // Check the latest creep list for any new creeps
        var newCreepList = new HashSet<ICreep>(_room.Room.Find<ICreep>().Where(static x => x.My));
        foreach (var creep in newCreepList) {
            var job = GetCreepJob(creep);
            if (job == null || !_creeps[job.Id].Add(creep)) {
                continue;
            }
            OnCreepSpawned(creep);
        }
        
        foreach (var spawn in _room.Spawns) {
            if (!spawn.Exists) {
                continue;
            }
            TickCreepSpawn(spawn);
        }

        // Tick all tracked creeps
        foreach (var creep in _creeps.SelectMany(c => c.Value)) {
            TickCreep(creep);
        }
    }

    private void OnCreepSpawned(ICreep creep) {
        // Get their job instance and sort them into the correct list
        var job = GetCreepJob(creep);
        if (job == null) {
            Console.WriteLine($"{this}: {creep} has unknown job!");
        } else {
            _creeps[job.Id].Add(creep);
        }
    }

    private void OnCreepDied(ICreep creep) {
        // Remove it from all tracking lists
        Console.WriteLine($"{this}: {creep} died");
    }

    private void TickCreepSpawn(IStructureSpawn spawn) {
        // Check if we're able to spawn something, and spawn until we've filled our target job counts
        if (spawn.Spawning != null) {
            return;
        }

        foreach (var (jobId, creeps) in _creeps)
        {
            var  job = _jobs[jobId];
            if (creeps.Count < _jobs[jobId].WantedCreepCount)
            {
                var bodyType = CalculateBodyType(job);
                if (bodyType != null)
                {
                    TrySpawnCreep(spawn, bodyType.Value, jobId);   
                }
                break;
            }
        }
    }

    private BodyType<BodyPartType>? CalculateBodyType(IJob job)
    {
        // we'll set all body part groups to their max value then decrease until their at their minimum
        var bodyPartsToCount = job.BodyPartGroups.ToDictionary(g => g, g => g.MaxCount);
        var costs = CalculateCosts(bodyPartsToCount);
        while (costs > _room.Room.EnergyAvailable)
        {
            var nothingChanged = true;
            foreach (var bodyPartCount in bodyPartsToCount)
            {
                if (bodyPartCount.Value > bodyPartCount.Key.MinCount)
                {
                    bodyPartsToCount[bodyPartCount.Key]--;
                    nothingChanged = false;
                    break;
                }
            }

            costs = CalculateCosts(bodyPartsToCount);

            if (nothingChanged)
            {
                // we could not decrease the count any further, so we'll live with it
                return null;
            }
        }
        return new BodyType<BodyPartType>(
            bodyPartsToCount.SelectMany(kv => kv.Key.BodyPartTypes.AsEnumerable().SelectMany(p => Enumerable.Repeat(p,kv.Value ))));
    }

    private int CalculateCosts(Dictionary<BodyPartGroup, int> bodyPartsToCount)
    {
        return bodyPartsToCount.Sum(kv => kv.Key.BodyPartTypes.Sum(t => _game.Constants.GetBodyPartCost(t))  * kv.Value);
    }

    // private static readonly BodyType<BodyPartType> workerBodyType = new([(BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1)]);
        
        

    private void TrySpawnCreep(IStructureSpawn spawn, BodyType<BodyPartType> bodyType, string jobName) {
        var name = FindUniqueCreepName();
        if (spawn.SpawnCreep(bodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok) {
            Console.WriteLine($"{this}: spawning a {jobName} ({bodyType}) from {spawn}...");
            var initialMemory = _game.CreateMemoryObject();
            initialMemory.SetValue("job", jobName);
            spawn.SpawnCreep(bodyType, name, new(dryRun: false, memory: initialMemory));
        }
    }

    private void TickCreep(ICreep creep) {
        // Lookup the job instance
        var job = GetCreepJob(creep);
        if (job == null) {
            return;
        }

        if (_game.Memory.GetConfigBool("showJobs")) {
            creep.Say(job.Icon);
        }

        // Run the job logic
        job.Run(creep);
    }

    private IJob? GetCreepJob(ICreep creep) {
        // First, see if we've stored the job instance on the creep from a previous tick (this will save us some CPU)
        if (creep.TryGetUserData<IJob>(out var job)) {
            return job;
        }
        
        var jobId = creep.GetJobId();
        if (jobId == null || !_jobs.TryGetValue(jobId, out job)) {
            return null;
        }

        // We found it, assign it to the creep user data for later retrieval
        creep.SetUserData(job);
        return job;
    }

    private string FindUniqueCreepName()
        => $"{_room.Room.Name}_{Random.Next()}";

    public override string ToString()
        => $"RoomManager[{_room.Room.Name}]";
}