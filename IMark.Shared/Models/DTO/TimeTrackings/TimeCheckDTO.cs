using IMark.Shared.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using TimeZoneConverter;
namespace IMark.Shared.Models.DTO.TimeTrackings;

public class TimeCheckDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp {  get; set; }
    public string TimeZoneId { get; set; } = string.Empty;

[NotMapped]
public DateTime TimestampLocal
{
    get
    {
        var id = TimeZoneId;

        // tenta direto (IANA no Linux/Android/iOS ou Windows válido)
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
            return TimeZoneInfo.ConvertTimeFromUtc(Timestamp, tz);

        // tenta converter Windows → IANA
        if (TZConvert.TryWindowsToIana(id, out var iana) &&
            TimeZoneInfo.TryFindSystemTimeZoneById(iana, out tz))
        {
            return TimeZoneInfo.ConvertTimeFromUtc(Timestamp, tz);
        }

        return Timestamp; // fallback seguro
    }

    set
    {
        var id = TimeZoneId;

        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
        {
            Timestamp = TimeZoneInfo.ConvertTimeToUtc(value, tz);
            return;
        }

        if (TZConvert.TryWindowsToIana(id, out var iana) &&
            TimeZoneInfo.TryFindSystemTimeZoneById(iana, out tz))
        {
            Timestamp = TimeZoneInfo.ConvertTimeToUtc(value, tz);
            return;
        }

        Timestamp = value.ToUniversalTime();
    }
}
}