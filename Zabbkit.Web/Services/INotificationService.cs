using Zabbkit.Web.Models;

namespace Zabbkit.Web.Services
{
    public interface INotificationService
    {
        void SendMessage(Device device, Message message, string trackingId);
        bool IsApnsChannelAccessable { get; }
        bool IsGcmChannelAccessable { get; }
        bool IsWpChannelAccessable { get; }
    }
}
