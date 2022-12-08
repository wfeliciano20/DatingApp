
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.interfaces;

using Microsoft.IdentityModel.Tokens;

namespace API.services
{
  public class TokenService : ITokenService
  {

    private readonly SymmetricSecurityKey _key;
    public TokenService(IConfiguration config)
    {
        _key =  new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }
    public string CreateToken(AppUser user)
    {
        // Generate the claims
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
        };

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
