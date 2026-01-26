using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheRocksNew.API.Models;

public class Picker
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public string FullName => $"{FirstName} {LastName}";
}

public class Orchard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal TotalArea { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OrchardBlock
{
    public Guid Id { get; set; }
    public Guid OrchardId { get; set; }
    
    // ADD THIS NAVIGATION PROPERTY:
    [JsonIgnore] // This prevents JSON serialization issues
    public virtual Orchard? Orchard { get; set; }
    
    public string BlockName { get; set; } = string.Empty;
    public string BlockNumber { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string AppleVariety { get; set; } = string.Empty;
    public string RoadNumber { get; set; } = string.Empty;
    public decimal DefaultBinRate { get; set; } = 45.00m;
    public bool IsActive { get; set; } = true;
    public string FullBlockName => $"{BlockName} {BlockNumber}";
}

public class ApplePrice
{
    public Guid Id { get; set; }
    public string Variety { get; set; } = string.Empty;
    public decimal PricePerKg { get; set; }
    public decimal BinRate { get; set; } = 45.00m;
    public DateTime EffectiveDate { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;
}

public class Packhouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class PickRecord
{
    public Guid Id { get; set; }
    public Guid PickerId { get; set; }
    
    // ADD THIS NAVIGATION PROPERTY:
    [JsonIgnore] // This prevents JSON serialization issues
    public virtual Picker? Picker { get; set; }
    
    public Guid OrchardBlockId { get; set; }
    
    // ADD THIS NAVIGATION PROPERTY:
    [JsonIgnore] // This prevents JSON serialization issues
    public virtual OrchardBlock? OrchardBlock { get; set; }
    
    public string AppleVariety { get; set; } = string.Empty;
    public int BinsPicked { get; set; }
    public decimal BinRate { get; set; }
    public decimal TotalAmount => BinsPicked * BinRate;
    public DateTime PickDate { get; set; }
}