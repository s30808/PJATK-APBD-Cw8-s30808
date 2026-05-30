using HospitalApp.DTOs;
using HospitalApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public PatientsController(HospitalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients([FromQuery] string? search)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => 
                    EF.Functions.Like(p.FirstName, $"%{search}%") || 
                    EF.Functions.Like(p.LastName, $"%{search}%")
                );
            }

            var patients = await query.Select(p => new PatientDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female",
                
                Admissions = p.Admissions.Select(a => new AdmissionDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),

                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedDto
                    {
                        Id = ba.Bed.Id,
                        BedType = new BedTypeDto
                        {
                            Id = ba.Bed.BedType.Id,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new RoomDto
                        {
                            Id = ba.Bed.Room.Id,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new WardDto
                            {
                                Id = ba.Bed.Room.Ward.Id,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            }).ToListAsync();

            return Ok(patients); 
        }
        
        [HttpPost("{pesel}/bedassignments")]
        public async Task<IActionResult> AssignBed(string pesel, [FromBody] AssignBedRequestDto request)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
            if (!patientExists)
            {
                return NotFound($"Pacjent o numerze PESEL {pesel} nie istnieje w bazie.");
            }

            var availableBed = await _context.Beds
                .Where(b => b.BedType.Name == request.BedType && b.Room.Ward.Name == request.Ward)
                .Where(b => !b.BedAssignments.Any(existingAssignment => 
                    (request.To == null || existingAssignment.From < request.To) &&
                    (existingAssignment.To == null || existingAssignment.To > request.From)
                ))
                .FirstOrDefaultAsync();

            if (availableBed == null)
            {
                return StatusCode(404, "Nie znaleziono wolnego łóżka o zadanym typie, na wskazanym oddziale, we wskazanym okresie czasu.");
            }

            var newAssignment = new BedAssignment
            {
                PatientPesel = pesel,
                BedId = availableBed.Id,
                From = request.From,
                To = request.To
            };

            _context.BedAssignments.Add(newAssignment);
            await _context.SaveChangesAsync();

            return Created("", new { Message = "Łóżko zostało pomyślnie przypisane.", AssignmentId = newAssignment.Id });
        }
    }
}