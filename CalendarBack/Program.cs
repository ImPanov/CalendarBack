using CalendarBack.Data;
using CalendarBack.Hubs;
using CalendarBack.Services;
using CalendarBack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using CalendarBack;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<CalendarEntry>("Calendar");

builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", modelBuilder.GetEdmModel()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Calendar API", 
        Version = "v1",
        Description = "API для работы с календарем и напоминаниями"
    });
    
    // Настройка для поддержки OData в Swagger
    c.DocInclusionPredicate((docName, apiDesc) => true);
    c.CustomSchemaIds(type => type.FullName);

    // Подключаем XML документацию
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Настройка для разрешения конфликтов в OData
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // Добавляем поддержку параметров OData
    c.OperationFilter<ODataOperationFilter>();
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<ReminderService>();

// Настройка для работы с UTC датами
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<CalendarDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// Применяем миграции и заполняем тестовые данные
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();
        context.Database.Migrate();
        DbSeeder.Initialize(context);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Произошла ошибка при инициализации базы данных");
}

app.Run();
