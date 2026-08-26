using KayraExport.Core.Entities;

namespace KayraExport.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}