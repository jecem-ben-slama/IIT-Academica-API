using IIT_Academica_API.Entities;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(int id);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<Notification>> GetAllAsync();
}
