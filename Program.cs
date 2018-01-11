using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string cognitoISS = "https://cognito-idp.<region>.amazonaws.com/<user-pool-id>"; // Your cognito ISS
            const string aud = ""; // Your API Client Identifier
            const string testToken = ""; // Obtain a JWT to validate and put it in here

            try
            {
                // Download the OIDC configuration which contains the JWKS
                // NB!!: Downloading this takes time, so do not do it very time you need to validate a token, Try and do it only once in the lifetime
                //     of your application!!
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{cognitoISS}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = AsyncHelper.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));

                // Configure the TokenValidationParameters. Assign the SigningKeys which were downloaded from Auth0. 
                // Also set the Issuer and Audience(s) to validate
                TokenValidationParameters validationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = cognitoISS,
                        ValidAudiences = new[] { aud },
                        IssuerSigningKeys = openIdConfig.SigningKeys
                    };

                // Now validate the token. If the token is not valid for any reason, an exception will be thrown by the method
                SecurityToken validatedToken;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var user = handler.ValidateToken(testToken, validationParameters, out validatedToken);

                // The ValidateToken method above will return a ClaimsPrincipal. Get the user ID from the NameIdentifier claim
                // (The sub claim from the JWT will be translated to the NameIdentifier claim)
                Console.WriteLine($"Token is validated. User Id {user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while validating token: {e.Message}");
                throw;
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}