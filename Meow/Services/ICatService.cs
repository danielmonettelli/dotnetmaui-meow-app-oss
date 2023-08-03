using Meow.Models;

namespace Meow.Services;

public interface ICatService
{
    Task<List<Cat>> GetRandomKitty();
}
