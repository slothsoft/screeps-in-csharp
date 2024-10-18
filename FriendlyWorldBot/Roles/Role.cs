using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Roles;

public interface IRole
{
    void Run(ICreep creep);
}