using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskViewModel, DBTask>();
        CreateMap<DBTask, TaskViewModel>();
        CreateMap<ApplicationUser, RegisterViewModel>();
        CreateMap<RegisterViewModel, ApplicationUser>();
    }
}