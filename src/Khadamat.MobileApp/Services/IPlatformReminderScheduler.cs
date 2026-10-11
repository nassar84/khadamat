using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.MobileApp.Services;

public interface IPlatformReminderScheduler
{
    void Initialize();
    void ScheduleNotification(ActiveClientReminderDto reminder);
    void CancelNotification(int reminderId);
    void CancelAllNotifications();
    Task<bool> RequestPermissionAsync();
}
