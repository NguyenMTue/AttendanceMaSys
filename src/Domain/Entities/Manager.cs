namespace MindVaultAI.Domain.Entities;

public class Manager : Employee
{
    public RoleEnum ManagerType { get; set; }

    public Manager()
    {
        Role = RoleEnum.DepartmentManager;
    }
}
