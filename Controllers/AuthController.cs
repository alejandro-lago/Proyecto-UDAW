using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
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
    private readonly IConfiguration configuration;

    public AuthController( UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
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

        var token = GenerateJwtToken(user);

        return Ok( new AuthResponse
        {
           Success = true,
           Message = "User Registered Succesfully",
           Token = token,
            User = new UserProfileResponse
            {
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                FirstName = user.FirstName,
                PhoneNumber = user.PhoneNumber
            }
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
        var token = GenerateJwtToken(user!);

        return Ok( new AuthResponse
        {
            Success = true,
            Message = "Login Successful",
            Token = token,
            User = new UserProfileResponse
            {
                Email = user.Email!,
                FullName = $"{user.FirstName} {user.LastName}",
                FirstName = user.FirstName,
                PhoneNumber = user.PhoneNumber
            }
        });
    }

    //  JWT (JSON Web Token) is a digital keycard, a string of text that proves who you are
    //  It has 3 parts, HEADER, PAYLOAD and SIGNATURE
    //  HEADER - Says, Im a JWT using this algorithm
    //  PAYLOAD - His data (userid, email, name)
    //  Why JWT and not cookies? browsers send cookies automatically, but REACT need to explicitly send the token in every request, and it seems that JWT it's the standard
    private string GenerateJwtToken(User user)
    {
        var jwtKey = configuration["Jwt:Key"]!; //  Gets config values (key, issuer, audience, expiry)
        var jwtIssuer = configuration["Jwt:Issuer"]!;
        var jwtAudience = configuration["Jwt:Audience"]!;
        var expireMinutes = int.Parse(configuration["Jwt:ExpireMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

