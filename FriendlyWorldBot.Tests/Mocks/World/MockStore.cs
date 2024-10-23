using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Tests.Mocks.World;

public class MockStore(int totalCapacity) : IStore {

    // https://docs.screeps.com/api/#StructureStorage
    public static int StructureStorageCapacity = 1_000_000;
    
    private readonly IDictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();
    
    public int? GetCapacity(ResourceType? resourceType = null) {
        return totalCapacity;
    }

    public int? GetUsedCapacity(ResourceType? resourceType = null) {
        if (resourceType == null) {
            return _resources.Values.Sum();
        }
        return _resources.TryGetValue(resourceType.Value, out var value) ? value : 0;
    }

    public int? GetFreeCapacity(ResourceType? resourceType = null) {
        return totalCapacity - GetUsedCapacity();
    }

    public IEnumerable<ResourceType> ContainedResourceTypes => _resources.Where(kv => kv.Value > 0).Select(kv => kv.Key);

    public int this[ResourceType resourceType] {
        get => _resources[resourceType];
        set => _resources[resourceType] = value;
    }
}