using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace MyApp.Helpers
{
    public static class JwtUtils
    {
        private const string PublicKeyPem =  @"
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn9YWfc3s8Q0dNuvK9axY
wM3m7zOCM9y46iuEuaE7drXFLFy8lSYsYyL3Era8byzwLZILwR6GXbSQYxW13XFO
N6l+kL0e6+3AXczdUFLpC85x7O5HNLXrXQY8P99zgDIM1Tub0U30rB1AL6+PDng9
z/X4Uvqk38SAKji+uvzlsDjRSiCfrMDJoZzeu5468veP3pCF71w4gNE7XxqB5RQl
xSKm9ev7oALJP1ZUp7m6fqMSD5Eo8jBg3i4gpesHA3f9iyiMcp0Uct+rwmaD3fy1
9WDAeTVbxcfJnItp3hYYJUX2toGCxkOhfFJMsuvb8cZkuPnf7m35QxKHqrewrO5a
uQIDAQAB
-----END PUBLIC KEY-----";

public static JwtSecurityToken ValidateAndDecode(string token)
{
    // load RSA from PEM…
    using var rsa = RSA.Create();
    rsa.ImportFromPem(PublicKeyPem.ToCharArray());
    var rsaKey = new RsaSecurityKey(rsa);

    var parameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKeyResolver = (tokenString, securityToken, kid, validationParams) =>
        {
            return new[] { rsaKey };
        },

        ValidateIssuer   = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ClockSkew        = TimeSpan.FromSeconds(30)
    };

    var handler = new JwtSecurityTokenHandler();
    handler.InboundClaimTypeMap.Clear();
    handler.ValidateToken(token, parameters, out var validated);
    return (JwtSecurityToken)validated;
}
    }
}