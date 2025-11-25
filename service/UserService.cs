
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
        public (bool Success, string? Role, Guid? UserId) Login(LoginDTO request)
        {
            var admin = _adminRepository.GetUserByEmail(request.email);
            if (admin != null && admin.password == request.password)
            {
                return (true, "ADMIN", admin.id);
            }

            var user = _userRepository.GetUserByEmail(request.email);
            if (user != null && user.password == request.password)
            {
                return (true, "USER", user.Id);
            }

            return (false, null, null);
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



        public bool CreateUsers(CreateUserAdminDTO dto, out string message)
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


        // ⚠️ ASSUMPTION: You have installed a hashing library like BCrypt.Net-Core.

        public bool ChangePassword(Guid id, String currentPassword, String newPassword)
        {
            // 1. Fetch user by ID
            var user = _userRepository.GetUserById(id);

            if (user == null)
            {
                // Fail if user not found
                return false;
            }

            // 2. SECURITY CHECK: Verify the current password
            // If passwords were securely hashed:
            // bool isCurrentPasswordCorrect = BCrypt.Net.BCrypt.Verify(currentPassword, user.password); 

            // Since your current system stores PLAINTEXT:
            bool isCurrentPasswordCorrect = (currentPassword == user.password);

            if (!isCurrentPasswordCorrect)
            {
                // Fail if current password does not match
                return false;
            }

            // 3. CRITICAL: Hash the new password before storing it (Industry Standard)
            // You MUST implement hashing here.
            // user.password = BCrypt.Net.BCrypt.HashPassword(newPassword); 

            // For now, using your existing PLAINTEXT assignment (TEMPORARY & INSECURE):
            user.password = newPassword;

            // 4. Save changes
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


        public bool UpdateUserInfo(EditUserDTO dto, out string message)
        {
            var user = _userRepository.GetUserByEmail(dto.Email);
            if (user == null)
            {
                message = "User not found";
                return false;
            }

            
            user.username = dto.Username;
            user.password = dto.Password;
            user.email = dto.Email;
            user.phone = dto.Phone;

            _userRepository.UpdateUser(user);

            message = "User updated successfully";
            return true;
        }



        // Inside WebApplication2.service/UserService.cs

        // ✅ NEW/REFACTORED DELETE METHOD
        public bool DeleteUserById(Guid id, out string message)
        {
            // 1. Find user by ID (instead of username)
            var user = _userRepository.GetUserById(id);

            if (user == null)
            {
                message = "User not found";
                return false;
            }

            // 2. Delete the user
            _userRepository.DeleteUser(user);

            message = "User deleted successfully";
            return true;
        }

        // NOTE: You can now remove the old DeleteUserByUsername method if it's no longer used.



        // We need to update the interface signature:
        // public UserProfileResult? GetUserProfileById(Guid id);

        public UserProfileResult? GetUserProfileById(Guid id)
        {
            var user = _userRepository.GetUserById(id);

            if (user == null)
                return null;

            // ✅ FINAL FIX: Project to a defined record structure 
            return new UserProfileResult(
                Id: user.Id,
                Username: user.username,
                Email: user.email,
                Phone: user.phone
            );
        }


        // ✅ FILE: UserService.cs (Implementation)

        // ✅ FILE: UserService.cs (Implementation) - Conditional Update

        public bool UpdateUserProfile(UpdateProfileDTO dto, out string message)
        {
            // 1. Fetch user by ID
            var user = _userRepository.GetUserById(dto.Id);
            if (user == null)
            {
                message = "User not found";
                return false;
            }

            // 2. Assign updated profile values (ONLY IF A NEW VALUE IS PROVIDED)
            if (!string.IsNullOrEmpty(dto.Username))
            {
                user.username = dto.Username;
            }

            // NOTE: Use dto.Email != null to allow the client to explicitly set email to NULL 
            // if your database allows it. If not, use string.IsNullOrEmpty.
            if (dto.Email != null)
            {
                user.email = dto.Email;
            }

            if (dto.Phone != null)
            {
                user.phone = dto.Phone;
            }

            // 3. Save changes
            _userRepository.UpdateUser(user);

            message = "User profile updated successfully";
            return true;
        }
    }
}
