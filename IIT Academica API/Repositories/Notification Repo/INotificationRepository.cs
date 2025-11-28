// Repositories/INotificationRepository.cs
using IIT_Academica_API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(int id);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(int id);

    // Student View (Read All)
    Task<IEnumerable<Notification>> GetAllAsync();
}