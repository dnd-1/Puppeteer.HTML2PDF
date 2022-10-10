using Microsoft.EntityFrameworkCore;
using Puppeteer_HTML2PDF.Data.Models.DTO;

namespace Puppeteer_HTML2PDF.Data;

public class ApplicationDbContext: DbContext
{
    public DbSet<PdfConvertingTask> Tasks { get; set; }   

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.Migrate();
    }
}