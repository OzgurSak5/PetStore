namespace PetStore.Domain.Settings;

public class OrderSettings
{
    public int ReservationMinutes { get; set; } = 15;
    public int ReservationCheckIntervalSeconds { get; set; } = 60;
}