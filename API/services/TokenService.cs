
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.services
{
  public class TokenService : ITokenService
  {
    private readonly UserManager<AppUser> _userManager;

    private readonly SymmetricSecurityKey _key;
    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
        _userManager = userManager;
        _key =  new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public async Task<string> CreateToken(AppUser user)
    {
        // Generate the claims
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        // get the roles
        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role,role)));

        // Generate Credentials
        var creds = new SigningCredentials(_key,SecurityAlgorithms.HmacSha512Signature);

        // Generate token descriptor

        var tokenDescriptor =  new SecurityTokenDescriptor
        {
            Subject  = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        // Get the token handler

        var tokenHandler =  new JwtSecurityTokenHandler();

        // Create the token

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
  }
}
