using AutoMapper;
using WebApplication2.models;
using WebApplication2.dto.userDTO;

namespace WebApplication2.mappers.userMapper
{
    public class MappingProfile :Profile
    {

      
        public MappingProfile() {

            //get
            CreateMap<User, GetUserDTO>();
            CreateMap<GetUserDTO, GetUserDTO>();

            //createUser
            CreateMap<CreateUserDTO,User>();



        }
    }
}
