using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public class CreepManager : IManager {

    private static IDictionary<BodyPartType, int> bodyPartTypeCosts = new Dictionary<BodyPartType, int> {
        {BodyPartType.Move, 50},
        {BodyPartType.Work, 100},
        {BodyPartType.Carry, 50},
        {BodyPartType.Attack, 80},
        {BodyPartType.RangedAttack, 150},
        {BodyPartType.Heal, 250},
        {BodyPartType.Tough, 10},
        {BodyPartType.Claim, 600},
    };
    
    internal static PolyVisualStyle? PathStyle; 
    private static readonly Random Random = new();
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly IDictionary<string, IJob> _jobs;
    private readonly IDictionary<string, ISet<ICreep>> _creeps;

    // TODO: remove these
    private static readonly BodyType<BodyPartType> workerBodyType = new([(BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1)]);

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

        foreach (var (jobId, creeps) in _creeps) {
            if (creeps.Count < _jobs[jobId].WantedCreepCount) {
                TrySpawnCreep(spawn, workerBodyType, jobId);
                break;
            }
        }
    }

    private void TrySpawnCreep(IStructureSpawn spawn, BodyType<BodyPartType> bodyType, string jobName) {
        var name = FindUniqueCreepName();
        if (spawn.SpawnCreep(bodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok) {
            Console.WriteLine($"{this}: spawning a {jobName} ({workerBodyType}) from {spawn}...");
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