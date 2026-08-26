using KayraExport.Core.Entities;

namespace KayraExport.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AppUser user,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}