namespace PetStore.Domain.Settings;

public class RateLimitSettings
{
    public int IpPermitLimit { get; set; } = 20;
    public int IpWindowMinutes { get; set; } = 5;
    public int EmailPermitLimit { get; set; } = 5;
    public int EmailWindowMinutes { get; set; } = 5;
}