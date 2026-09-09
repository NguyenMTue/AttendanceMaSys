namespace MindVaultAI.Domain.Entities;

public class QA : Employee
{
    public int Band { get; set; }
    public bool CodingSkillsFlag { get; set; }

    public QA()
    {
        Role = RoleEnum.Employee;
    }
}
