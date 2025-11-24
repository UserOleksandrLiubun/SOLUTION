using CHOICE;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TaskViewModel
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? EditorId { get; set; }
    [Display(Name = "Виконати до")]
    public DateTime? DueDate { get; set; }
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [Display(Name = "Назва")]
    public string Name { get; set; }
    [Display(Name = "Опис")]
    [StringLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
    [Display(Name = "Важливо")]
    public bool IsImportant { get; set; }
    [Display(Name = "Завершено")]
    public bool IsCompleted { get; set; }
}