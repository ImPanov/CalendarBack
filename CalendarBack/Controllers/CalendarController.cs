using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Results;
using CalendarBack.Data;
using CalendarBack.Models;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace CalendarBack.Controllers;

/// <summary>
/// Контроллер для работы с календарными записями
/// </summary>
[ApiController]
[Route("odata/[controller]")]
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
    /// <remarks>
    /// Примеры запросов:
    /// GET /odata/Calendar?$filter=ReminderDateTime gt 2024-02-23
    /// GET /odata/Calendar?$orderby=ReminderDateTime desc
    /// GET /odata/Calendar?$select=Title,ReminderDateTime
    /// GET /odata/Calendar?$top=5
    /// </remarks>
    [EnableQuery]
    [HttpGet]
    [ProducesResponseType(typeof(IQueryable<CalendarEntry>), 200)]
    [ApiExplorerSettings(GroupName = "GetAll")]
    public IQueryable<CalendarEntry> GetAll()
    {
        return _context.CalendarEntries;
    }

    /// <summary>
    /// Получить календарную запись по ID
    /// </summary>
    /// <param name="id">ID записи</param>
    [EnableQuery]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SingleResult<CalendarEntry>), 200)]
    [ProducesResponseType(404)]
    [ApiExplorerSettings(GroupName = "GetById")]
    public SingleResult<CalendarEntry> GetById(int id)
    {
        var result = _context.CalendarEntries.Where(e => e.Id == id);
        return SingleResult.Create(result);
    }

    /// <summary>
    /// Создать новую календарную запись
    /// </summary>
    /// <param name="entry">Данные записи</param>
    [HttpPost]
    [ProducesResponseType(typeof(CalendarEntry), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CalendarEntry>> Post([FromBody] CalendarEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        _context.CalendarEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Created(entry);
    }

    /// <summary>
    /// Обновить существующую календарную запись
    /// </summary>
    /// <param name="id">ID записи</param>
    /// <param name="entry">Обновленные данные</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CalendarEntry), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Put(int id, [FromBody] CalendarEntry entry)
    {
        if (id != entry.Id)
        {
            return BadRequest();
        }

        var existingEntry = await _context.CalendarEntries.FindAsync(id);
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
            if (!EntryExists(id))
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
    /// <param name="id">ID записи</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.CalendarEntries.FindAsync(id);
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
    [ProducesResponseType(typeof(FileResult), 200)]
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

    private bool EntryExists(int id)
    {
        return _context.CalendarEntries.Any(e => e.Id == id);
    }
}
