using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
using CalendarBack.Data;
using CalendarBack.Models;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace CalendarBack.Controllers;

public class CalendarController : ODataController
{
    private readonly CalendarDbContext _context;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(CalendarDbContext context, ILogger<CalendarController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех календарных записей
    /// </summary>
    [EnableQuery]
    public IQueryable<CalendarEntry> Get()
    {
        return _context.CalendarEntries;
    }

    /// <summary>
    /// Получить календарную запись по ID
    /// </summary>
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var entry = await _context.CalendarEntries.FindAsync(key);
        if (entry == null)
        {
            return NotFound();
        }
        return Ok(entry);
    }

    /// <summary>
    /// Создать новую календарную запись
    /// </summary>
    public async Task<IActionResult> Post([FromBody] CalendarEntry entry)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        entry.CreatedAt = DateTime.UtcNow;
        _context.CalendarEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Created(entry);
    }

    /// <summary>
    /// Обновить существующую календарную запись
    /// </summary>
    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] CalendarEntry entry)
    {
        if (key != entry.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingEntry = await _context.CalendarEntries.FindAsync(key);
        if (existingEntry == null)
        {
            return NotFound();
        }

        existingEntry.Title = entry.Title;
        existingEntry.Description = entry.Description;
        existingEntry.ReminderDateTime = entry.ReminderDateTime;
        existingEntry.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await EntryExists(key))
            {
                return NotFound();
            }
            throw;
        }

        return Updated(existingEntry);
    }

    /// <summary>
    /// Удалить календарную запись
    /// </summary>
    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        var entry = await _context.CalendarEntries.FindAsync(key);
        if (entry == null)
        {
            return NotFound();
        }

        _context.CalendarEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Экспортировать все записи в CSV формат
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv()
    {
        var entries = await _context.CalendarEntries.OrderBy(e => e.ReminderDateTime).ToListAsync();
        
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(entries);
        await writer.FlushAsync();
        
        var bytes = memoryStream.ToArray();
        return File(bytes, "text/csv", $"calendar-export-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    private async Task<bool> EntryExists(int key)
    {
        return await _context.CalendarEntries.AnyAsync(e => e.Id == key);
    }
}
