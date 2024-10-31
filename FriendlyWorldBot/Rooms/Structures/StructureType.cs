using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public struct StructureType {
    public static readonly StructureType Tower = new("towers", s => s is IStructureTower);
    public static readonly StructureType Container = new("containers", s => s is IStructureContainer);

    private static IList<StructureType> _all;
    public static ICollection<StructureType> All => _all;
    
    private readonly string _collectionName;
    private readonly Func<IStructure, bool> _isAssignableFrom;

    private StructureType(string collectionName, Func<IStructure, bool> isAssignableFrom) {
        _collectionName = collectionName;
        _isAssignableFrom = isAssignableFrom;
        
        _all = new List<StructureType>();
        _all.Add(this);
    }

    public string CollectionName => _collectionName;
    public bool IsAssignableFrom(IStructure structure) => _isAssignableFrom(structure);
}