using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace iSchool.Infras.Tokens
{

    public static class JwtUtil
    {
        static JwtSecurityTokenHandler JwtSecurityTokenHandler => new Ex_JwtSecurityTokenHandler();

        public static SigningCredentials CreateSigningCredentials(byte[] key, string alg = SecurityAlgorithms.HmacSha256) => new SigningCredentials(new SymmetricSecurityKey(key), alg);

        public static SigningCredentials CreateSigningCredentials(string key, string alg = SecurityAlgorithms.HmacSha256) => CreateSigningCredentials(Encoding.UTF8.GetBytes(key), alg);

        public static JwtHeader CreateHeader() => new JwtHeader();

        public static JwtHeader CreateHeader(SigningCredentials signingCredentials) => new JwtHeader(signingCredentials);

        public static JwtHeader CreateHeader(byte[] key, string alg = SecurityAlgorithms.HmacSha256) => new JwtHeader(CreateSigningCredentials(key, alg));

        public static JwtHeader CreateHeader(string key, string alg = SecurityAlgorithms.HmacSha256) => CreateHeader(Encoding.UTF8.GetBytes(key), alg);

        public static JwtHeader AddHeader(this JwtHeader jwtHeader, string k, object v)
        {
            if (jwtHeader != null) jwtHeader[k] = v;
            return jwtHeader;
        }

        public static JwtHeader DelHeader(this JwtHeader jwtHeader, string k)
        {
            jwtHeader?.Remove(k);
            return jwtHeader;
        }

        public static JwtPayload CreatePayload() => new JwtPayload(null, null, null, null, null, null);

        public static JwtPayload CreatePayload(TimeSpan exp)
        {
            var now = DateTime.Now;
            return new JwtPayload(null, null, null, now, now.Add(exp), null);
        }

        public static JwtPayload CreatePayload(double exp)
        {
            var now = DateTime.Now;
            return new JwtPayload(null, null, null, now, now.AddSeconds(exp), null);
        }

        public static JwtPayload AddClaim(this JwtPayload jwtPayload, string key, object value)
        {
            jwtPayload?.AddClaim(new Claim(key, value.ToString()));
            return jwtPayload;
        }

        public static JwtPayload DelClaim(this JwtPayload jwtPayload, string key)
        {
            jwtPayload?.Remove(key);
            return jwtPayload;
        }

        public static string CreateSignature(string base64urlHeader, string base64urlPayload, SigningCredentials signingCredentials)
        {
            return signingCredentials == null ? string.Empty : JwtTokenUtilities.CreateEncodedSignature(string.Concat(base64urlHeader, ".", base64urlPayload), signingCredentials);
        }

        public static string CreateJwtStr(SigningCredentials signingCredentials, JwtHeader header, JwtPayload payload)
        {
            var newHeader = new JwtHeader(signingCredentials);
            newHeader.Clear();
            foreach (var kv in header)
                newHeader[kv.Key] = kv.Key;

            return CreateJwtStr(newHeader, payload);
        }

        public static string CreateJwtStr(JwtHeader header, JwtPayload payload)
        {
            return JwtSecurityTokenHandler.WriteToken(new JwtSecurityToken(header, payload));
        }

        public static JwtSecurityToken CreateJwt(JwtHeader header, JwtPayload payload) => new JwtSecurityToken(header, payload);

        public static JwtSecurityToken GetJwtToken(string jwt, TokenValidationParameters validationParameters = null)
        {
            if (validationParameters == null) return new JwtSecurityToken(jwt); //JwtSecurityTokenHandler.ReadJwtToken(jwt);
            JwtSecurityTokenHandler.ValidateToken(jwt, validationParameters, out var vtoken);
            return vtoken as JwtSecurityToken;
        }

        public static (ClaimsPrincipal, JwtSecurityToken) ValidJwt(string jwt, TokenValidationParameters validationParameters)
        {
            var cp = JwtSecurityTokenHandler.ValidateToken(jwt, validationParameters, out var vtoken);
            if (cp?.Identity?.IsAuthenticated != true) return (null, null);
            return (cp, vtoken as JwtSecurityToken);
        }

        class Ex_JwtSecurityTokenHandler : JwtSecurityTokenHandler
        {
            //public ClaimsPrincipal ValidJwt(JwtSecurityToken jwtSecurityToken)
            //{
            //
            //}
        }

        public static TokenValidationParameters CreateValidParams(SigningCredentials signingCredentials = null, double? exp = null)
        {
            return new TokenValidationParameters
            {
                ValidateAudience = false,
                //ValidAudience = "the audience you want to validate",
                ValidateIssuer = false,
                //ValidIssuer = "the isser you want to validate",

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingCredentials?.Key,
                //TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(@"!!!!!!!")),

                ValidateLifetime = exp != null, //validate the expiration and not before values in the token
                ClockSkew = exp != null ? TimeSpan.FromSeconds(exp.Value) : default, //5 minute tolerance for the expiration date

                //NameClaimType = Consts.Auth.JWT_UerId, //use for `HttpContext.User.Identity.Name`
            };
        }
    }
}
