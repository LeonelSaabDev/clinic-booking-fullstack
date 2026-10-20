using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Doctors;

public sealed record ScheduleBlockDto(DayOfWeek Day, TimeOnly Start, TimeOnly End);

public sealed record UpdateDoctorScheduleRequest(int AppointmentDurationMinutes, IReadOnlyList<ScheduleBlockDto>? Blocks);

public sealed record DoctorScheduleResponse(int AppointmentDurationMinutes, IReadOnlyList<ScheduleBlockDto> Blocks)
{
    public static DoctorScheduleResponse FromEntity(Doctor doctor) =>
        new(
            doctor.AppointmentDurationMinutes,
            doctor.WeeklySchedule
                .OrderBy(b => b.Day)
                .ThenBy(b => b.Start)
                .Select(b => new ScheduleBlockDto(b.Day, b.Start, b.End))
                .ToList());
}
