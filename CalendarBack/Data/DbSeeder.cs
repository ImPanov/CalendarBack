using CalendarBack.Models;

namespace CalendarBack.Data;

public static class DbSeeder
{
    public static void Initialize(CalendarDbContext context)
    {
        if (context.CalendarEntries.Any())
        {
            return;
        }

        var baseDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var entries = new List<CalendarEntry>();

        // Создаем записи на февраль 2025
        for (int i = 0; i < 10; i++)
        {
            entries.Add(new CalendarEntry
            {
                Title = $"Встреча {i + 1}",
                Description = $"Описание встречи {i + 1}",
                ReminderDateTime = baseDate.AddDays(i * 3), // каждые 3 дня
                CreatedAt = DateTime.UtcNow,
            });
        }

        // Создаем записи на март 2025
        baseDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 10; i++)
        {
            entries.Add(new CalendarEntry
            {
                Title = $"Событие {i + 1}",
                Description = $"Описание события {i + 1}",
                ReminderDateTime = baseDate.AddDays(i * 2), // каждые 2 дня
                CreatedAt = DateTime.UtcNow,
            });
        }

        context.CalendarEntries.AddRange(entries);
        context.SaveChanges();
    }
}
