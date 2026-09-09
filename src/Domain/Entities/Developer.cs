namespace MindVaultAI.Domain.Entities;

public class Developer : Employee
{
    public int Band { get; set; }
    public string TechnicalDirection { get; set; } = string.Empty;

    public Developer()
    {
        Role = RoleEnum.Employee;
    }
}
