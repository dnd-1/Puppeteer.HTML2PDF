using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Puppeteer_HTML2PDF.Data;
using Puppeteer_HTML2PDF.Data.Models.Properties;
using Puppeteer_HTML2PDF.Jobs;
using Puppeteer_HTML2PDF.Services;
using Quartz;

if (!string.IsNullOrEmpty(Environment.ProcessPath))
    Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceProperties>(builder.Configuration.GetSection("ServiceProperties"));


builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DatabaseConnection")), ServiceLifetime.Singleton
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(logging => { logging.AddLog4Net(); });

builder.Services.AddHostedService<PuppeteerConverterService>();

builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
                
builder.Services.AddQuartz(quartz =>
{
    quartz.UseMicrosoftDependencyInjectionJobFactory();
    quartz.UseSimpleTypeLoader();
    quartz.UseInMemoryStore();

    quartz.UseDefaultThreadPool(treadPoolOptions => { treadPoolOptions.MaxConcurrency = 10; });

    string startDeleteCronExpression =
        builder.Configuration.GetValue("ServiceProperties:StartDeleteCronExpression",
            "0 0 7 * * ?");

    quartz.ScheduleJob<RemoveOldTasksJob>(trigger => trigger
        .WithIdentity("RemoveOldTasksJob")
        .WithCronSchedule(startDeleteCronExpression)
        .WithDescription("Job for delete old record from database")
    );
                    
});
        
builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
    options.AwaitApplicationStarted = true;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Environment.CurrentDirectory, "out")),
    RequestPath = "/out"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Environment.CurrentDirectory, "Pages")),
    RequestPath = ""
});


app.MapControllers();


app.Run();
