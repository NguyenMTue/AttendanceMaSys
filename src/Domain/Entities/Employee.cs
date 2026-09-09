namespace MindVaultAI.Domain.Entities;

public class Employee : BaseEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public Department Department { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsIntern { get; set; }
    public RoleEnum Role { get; set; }
}
