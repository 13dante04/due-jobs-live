namespace DueJobs.Api;

// A local stand-in for an external delivery service. No messages leave this process.
public interface IDeliveryGateway
{
    void SendEmail(string address, string message);
    void PostWebhook(string url, string message);
}

public sealed class RecordingDeliveryGateway : IDeliveryGateway
{
    private readonly List<string> _deliveries = new();
    public IReadOnlyList<string> Deliveries => _deliveries.ToArray();
    public void SendEmail(string address, string message) => _deliveries.Add($"Email:{address}:{message}");
    public void PostWebhook(string url, string message) => _deliveries.Add($"Webhook:{url}:{message}");
}
