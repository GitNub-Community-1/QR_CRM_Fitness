using System.ComponentModel.DataAnnotations;
using Markdig.Helpers;

namespace TestQRApp.Models.Entity_s;

public class Trainer
{
    public Guid Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Specialization { get; set; }

    public ICollection<Client> Clients { get; set; } = new List<Client>();
}