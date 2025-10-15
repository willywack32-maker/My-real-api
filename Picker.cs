using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SharedModels
{
    public class Picker
    {
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("name")] // Add this to match JSON property names
        public required string Name { get; set; }

        [JsonPropertyName("appleType")]
        public string? AppleType { get; set; }

        [JsonPropertyName("orchardName")]
        public required string OrchardName { get; set; }

        [JsonPropertyName("hoursWorked")]
        public decimal? HoursWorked { get; set; }

        [JsonPropertyName("binRate")]
        public decimal? BinRate { get; set; }

        [JsonPropertyName("packHouse")]
        public required string PackHouse { get; set; }
    }
}