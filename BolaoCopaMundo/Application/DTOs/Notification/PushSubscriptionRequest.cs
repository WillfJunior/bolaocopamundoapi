namespace BolaoCopaMundo.Application.DTOs.Notification;

public record PushSubscriptionRequest(string Endpoint, string P256dh, string Auth, string? DeviceInfo);
