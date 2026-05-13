using Microsoft.AspNetCore.Identity;

namespace ClinicAPI.Models;

// ⚠ STUB — This file will be fully replaced in Step 7.
// It exists now so the solution compiles.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}