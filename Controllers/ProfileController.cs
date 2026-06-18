using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;

namespace TfgApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<User> userManager;

    public ProfileController(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        return Ok(MapProfile(user));
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var nameParts = request.FullName.Split(' ', 2);
            user.FirstName = nameParts[0];
            user.LastName = nameParts.Length > 1 ? nameParts[1] : "";
        }

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Utc);

        if (request.Bio != null)
            user.Bio = request.Bio;

        if (request.ProfilePictureUrl != null)
            user.ProfilePictureUrl = request.ProfilePictureUrl;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Failed to update profile",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        return Ok(MapProfile(user));
    }

    private static UserProfileResponse MapProfile(User user)
    {
        return new UserProfileResponse
        {
            Email = user.Email ?? "",
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            FirstName = user.FirstName,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            DateOfBirth = user.DateOfBirth
        };
    }
}
