using clock.Services;

namespace clock.Components.Shared;

public static class StateLabels
{
    public static string Alarm(AlarmState state) => state switch
    {
        AlarmState.Active => "Включён", AlarmState.Disabled => "Выключен", _ => "Сработал"
    };
    public static string Run(RunState state) => state switch
    {
        RunState.Running => "Идёт отсчёт", RunState.Paused => "На паузе",
        RunState.Finished => "Завершён", _ => "Остановлен"
    };
    public static string Day(AlarmDays day) => day switch
    {
        AlarmDays.Monday => "Понедельник", AlarmDays.Tuesday => "Вторник",
        AlarmDays.Wednesday => "Среда", AlarmDays.Thursday => "Четверг",
        AlarmDays.Friday => "Пятница", AlarmDays.Saturday => "Суббота", _ => "Воскресенье"
    };
    public static string ShortDay(AlarmDays day) => day switch
    {
        AlarmDays.Monday => "Пн", AlarmDays.Tuesday => "Вт", AlarmDays.Wednesday => "Ср",
        AlarmDays.Thursday => "Чт", AlarmDays.Friday => "Пт", AlarmDays.Saturday => "Сб", _ => "Вс"
    };
    public static string Days(AlarmDays days) => days == AlarmDays.None ? "Однократно" :
        string.Join(", ", AlarmSchedule.Choices.Where(choice => (days & choice.Day) != 0)
            .Select(choice => ShortDay(choice.Day)));
}
