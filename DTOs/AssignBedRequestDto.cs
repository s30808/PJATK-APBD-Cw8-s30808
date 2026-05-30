namespace HospitalApp.DTOs;

public class AssignBedRequestDto
{
    public DateTime From { get; set; }
    public DateTime? To { get; set; } 
    public string BedType { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
}
public class WardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class BedTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RoomDto
{
    public string Id { get; set; } = string.Empty;
    public bool HasTv { get; set; }
    public WardDto Ward { get; set; } = null!;
}

public class BedDto
{
    public int Id { get; set; }
    public BedTypeDto BedType { get; set; } = null!;
    public RoomDto Room { get; set; } = null!;
}

public class AdmissionDto
{
    public int Id { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public WardDto Ward { get; set; } = null!;
}

public class BedAssignmentDto
{
    public int Id { get; set; }
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public BedDto Bed { get; set; } = null!;
}

public class PatientDto
{
    public string Pesel { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty; // Male lub Female
    public List<AdmissionDto> Admissions { get; set; } = new();
    public List<BedAssignmentDto> BedAssignments { get; set; } = new();
}