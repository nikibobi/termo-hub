using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace TermoHub.Options
{
    public class JwtOptions
    {
        public string Key { get; set; }
        public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiresDays { get; set; }

        public DateTime Expires => DateTime.Now.AddDays(ExpiresDays);
        public SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        public SigningCredentials Credentials => new SigningCredentials(SecurityKey, Algorithm);
    }
}
