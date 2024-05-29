
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WatcherApi.Classes;

namespace WatcherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
 
    public class IdentityAPIController : ControllerBase
    {
        private readonly Jwtsettings _jwtsettings;
        private readonly Context _context;

        public IdentityAPIController(IOptions <Jwtsettings> jwtsettings, Context context)
        {
            _jwtsettings = jwtsettings.Value;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("Giris")]
        public IActionResult Giris([FromBody]Admin apiKullanıcıBilgileri)
        {
            var apiKullanicisi = KimlikDenetimiYap(apiKullanıcıBilgileri);
            if (apiKullanicisi == null) return NotFound("Kullanıcı Bulunamadı.");

            var token = TokenOlustur(apiKullanicisi);
            return Ok(token);

        }

        private string TokenOlustur(Admin apiKullanicisi) //key ihtiyacımız var.
        {
            if (_jwtsettings.Key == null) throw new Exception("Jwt ayarlarındaki Key değeri null olamaz.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimDizisi = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, apiKullanicisi.Username ),   //Hangi bilgileri istiyorsam onalrı yazıyorum
                //new Claim(ClaimTypes.Role, apiKullanicisi.Role)
            };

            //tOKEN OLUŞTURMA.

            var token = new JwtSecurityToken(_jwtsettings.Issuer,
                _jwtsettings.Audience,
                claimDizisi,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
               
        }

        private Admin? KimlikDenetimiYap(Admin apiKullanıcıBilgileri) //Listeden girdiğimiz kullanıcıya uyan veri var mı diye bakacak
        {
            return _context
                .Admins
                .FirstOrDefault(x => x.Username.ToLower() == apiKullanıcıBilgileri.Username
                && x.Password == apiKullanıcıBilgileri.Password 
                //&& x.Role == apiKullanıcıBilgileri.Role
                );
                
        }
    }
}
