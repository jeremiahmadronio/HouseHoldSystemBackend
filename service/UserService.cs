
using AutoMapper;
using WebApplication2.dto.userDTO;
using WebApplication2.repositories;
using WebApplication2.models;


namespace WebApplication2.service

{
    public class UserService : IUserService
    {
        private readonly  IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository , IAdminRepository adminRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _adminRepository = adminRepository;
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
        public (bool Success, string? Role) Login(LoginDTO request)
        {
            var admin = _adminRepository.GetUserByEmail(request.email);
            if (admin != null && admin.password == request.password)
            {
                return (true, "ADMIN");
            }

            var user = _userRepository.GetUserByEmail(request.email);
            if (user != null && user.password == request.password)
            {
                return (true, "USER");
            }

            return (false, null);
        }

        //create User
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


        public bool ResetPassword(String email, String password) {

            var user = _userRepository.GetUserByEmail(email);

            if (user == null) return false;

            user.password = password;
            _userRepository.UpdateUser(user);

            return true;
        }


        public UserProfileDTO? GetUserProfile(String email) {

            var user = _userRepository.GetUserByEmail(email);
            if (user == null) return null;

            return _mapper.Map<UserProfileDTO>(user);
            {
                
            }

        }
        

    }
}
