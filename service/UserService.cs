
using AutoMapper;
using WebApplication2.dto.userDTO;
using WebApplication2.repositories;
using WebApplication2.models;


namespace WebApplication2.service

{
    public class UserService : IUserService
    {
        private readonly  IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository , IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }


        //get all user
        public async Task<IEnumerable<GetUserDTO>> getAllAsync()
        {
            var users = await _userRepository.getAllAsync();         
            return _mapper.Map<List<GetUserDTO>>(users);
            
        }
        //verify email
        public bool verifyEmail(String email) 
        { 
            return _userRepository.EmailExists(email);
        
        }

        //login
        public bool Login(LoginDTO request) {
        
            var user = _userRepository.GetUserByEmail(request.email);
            if (user == null || user.password != request.password ) { 

                return false;
            }
            return true;
        }

        public bool CreateUser(CreateUserDTO dto, out string message)
        {
            if (_userRepository.GetUserByEmail(dto.email) != null)
            {
                message = "Email already exists";
                return false;
            }

            var user = _mapper.Map<User>(dto);
            _userRepository.AddUser(user);

            message = "User created successfully";
            return true;
        }


    }
}
