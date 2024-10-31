using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public interface IPurpose {
    void Run(IStructure structure);
}

public abstract class BasePurpose<TStructure> : IPurpose 
    where TStructure: IStructure
{
    public void Run(IStructure structure) {
        RunForStructure((TStructure) structure);
    }

    protected abstract void RunForStructure(TStructure structure);
}