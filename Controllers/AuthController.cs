using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TfgApi.Models;

namespace TfgApi.Controllers;

[ApiController] //  Tells the ASP.NET that this class will handle API requests
[Route("api/[controller]")] //  Sets the URL
public class AuthController : ControllerBase    // ControllBase has helpers such as Ok(), BadRequest(), etc
{
    private readonly UserManager<User> userManager; //  Identity's class to create/find users in the DB
    private readonly SignInManager<User> signInManager; //  Identity's class to check passwords and log users in

    public AuthController( UserManager<User> userManager, SignInManager<User> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        // Split the full name into first and last names
        var nameParts = request.FullName.Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        var user = new User
        {
          UserName = request.Email,
          Email = request.Email,
          FirstName = firstName,
          LastName = lastName  
        };

        //  CreateAsync will save the user and hashes the password
        var result = await userManager.CreateAsync(user, request.Password);

        if(!result.Succeeded)
        {
            return BadRequest( new AuthResponse
            {
                Success = false,
                Message = "Registration Failed",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        return Ok( new AuthResponse
        {
           Success = true,
           Message = "User Registered Succesfully" 
        });
    }
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        // PasswordSignInAscyn checks the email exists and that the password matches the hash
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

        if (!result.Succeeded)
        {
            return Unauthorized ( new AuthResponse
            {
               Success = false,
               Message = "Invalid Email or Password" 
            });
        }

        var user = await userManager.FindByEmailAsync(request.Email);

        return Ok( new AuthResponse
        {
            Success = true,
            Message = "Login Successful",
            User = new UserProfileResponse
            {
                Id = user!.Id,
                Email = user.Email!,
                FullName = $"{user.FirstName} {user.LastName}"
            }
        });
    }
}

