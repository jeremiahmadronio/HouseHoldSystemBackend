// ✅ NEW FILE: WebApplication2/dto/userDTO/UserProfileResult.cs

namespace WebApplication2.dto.userDTO
{
	// Note: We use 'public record' so it's globally visible.
	public record UserProfileResult(
		Guid Id,
		string Username,
		string? Email,
		string? Phone
	);
}