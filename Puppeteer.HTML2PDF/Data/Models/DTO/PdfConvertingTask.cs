using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Puppeteer_HTML2PDF.Data.Models.DTO;

public class PdfConvertingTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ConvertingState State { get; set; }
    public Uri? FromUrl { get; set; }
    // public Uri FromFilePath { get; set; } = null!;
    public string OutPdfFileName { get; set; } = null!;

    [NotMapped]
    public virtual string OutPdfFileFullName { get; set; } = null!;
    public DateTime? FinishDate { get; set; }

}