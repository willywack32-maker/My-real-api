using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Picker
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("appleType")]
    public string? AppleType { get; set; }

    [Required]
    [JsonPropertyName("orchardName")]
    public string OrchardName { get; set; } = string.Empty;

    [JsonPropertyName("hoursWorked")]
    public decimal? HoursWorked { get; set; }

    [JsonPropertyName("binRate")]
    public decimal? BinRate { get; set; }

    [Required]
    [JsonPropertyName("packHouse")]
    public string PackHouse { get; set; } = string.Empty;
}