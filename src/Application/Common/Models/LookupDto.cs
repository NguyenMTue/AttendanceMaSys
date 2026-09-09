using AttendanceMaSys.Domain.Entities;

namespace AttendanceMaSys.Application.Common.Models;

public class LookupDto
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Employee, LookupDto>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName));
        }
    }
}
