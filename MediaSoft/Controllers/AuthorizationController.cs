using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using MediaSoft.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Mvc.Internal;
using OpenIddict.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediaSoft.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<Radnik> _signInManager;
        private readonly SignInManager<Worker> _signInManagerWorker;
        private readonly UserManager<Worker> _userManagerWorker;
        private readonly UserManager<Radnik> _userManager;

        public AuthorizationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<Radnik> signInManager,
            SignInManager<Worker> signInManagerWorker,
            UserManager<Radnik> userManager,
            UserManager<Worker> userManagerWorker
            )
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _signInManagerWorker = signInManagerWorker;
            _userManager = userManager;
            _userManagerWorker = userManagerWorker;
        }


        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange([ModelBinder(typeof(OpenIddictMvcBinder))] OpenIdConnectRequest request, string userType)
        {
            if (request.IsPasswordGrantType())
            {
                Radnik radnik = null;
                Worker worker = null;

                if (userType.Equals("radnik"))
                {
                    radnik = await _userManager.FindByNameAsync(request.Username);
                }
                else if (userType.Equals("worker"))
                {
                    worker = await _userManagerWorker.FindByNameAsync(request.Username);
                }
                
                if (radnik == null && worker == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                Microsoft.AspNetCore.Identity.SignInResult result = null;
                if (userType.Equals("radnik"))
                {
                    result = await _signInManager.CheckPasswordSignInAsync(radnik, request.Password, lockoutOnFailure: true);
                }
                else if (userType.Equals("worker"))
                {
                    result = await _signInManagerWorker.CheckPasswordSignInAsync(worker, request.Password, lockoutOnFailure: true);
                }
                
                if (!result.Succeeded)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                AuthenticationTicket ticket = null;
                // Create a new authentication ticket.
                if (userType.Equals("worker"))
                {
                    ticket = await CreateTicketWorkerAsync(request, worker);
                }
                else if (userType.Equals("radnik"))
                {
                    ticket = await CreateTicketAsync(request, radnik);
                }
                

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                // Returns the user corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType
                //     claim in the principal or null.

                var user = await _userManager.GetUserAsync(info.Principal);
                // Note: if you want to automatically invalidate the refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);

                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid."
                    });
                }

                // Ensure the user is still allowed to sign in.
                // returns boolean
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Create a new authentication ticket, but reuse the properties stored
                // in the refresh token, including the scopes originally granted.
                var ticket = await CreateTicketAsync(request, user, info.Properties);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        //[HttpPost("~/connect/tokenWorker"), Produces("application/json")]
        //public async Task<IActionResult> Razmeni([ModelBinder(typeof(OpenIddictMvcBinder))] OpenIdConnectRequest request)
        //{
        //    if (request.IsPasswordGrantType())
        //    {
        //        var user = await _userManagerWorker.FindByNameAsync(request.Username);
        //        if (user == null)
        //        {
        //            return BadRequest(new OpenIdConnectResponse
        //            {
        //                Error = OpenIdConnectConstants.Errors.InvalidGrant,
        //                ErrorDescription = "The username/password couple is invalid."
        //            });
        //        }

        //        // Validate the username/password parameters and ensure the account is not locked out.
        //        var result = await _signInManagerWorker.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        //        if (!result.Succeeded)
        //        {
        //            return BadRequest(new OpenIdConnectResponse
        //            {
        //                Error = OpenIdConnectConstants.Errors.InvalidGrant,
        //                ErrorDescription = "The username/password couple is invalid."
        //            });
        //        }

        //        // Create a new authentication ticket.
        //        var ticket = await CreateTicketWorkerAsync(request, user);

        //        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        //    }

        //    return BadRequest(new OpenIdConnectResponse
        //    {
        //        Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
        //        ErrorDescription = "The specified grant type is not supported."
        //    });
        //}

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, Radnik user, AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIddictServerDefaults.AuthenticationScheme);

            if (!request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
            }
            var claimsIdentity = (ClaimsIdentity)ticket.Principal.Identity;
            claimsIdentity.AddClaim(new Claim("nivo_pristupa", "radnik"));
            ticket.SetResources("resource_server");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }
        private async Task<AuthenticationTicket> CreateTicketWorkerAsync(OpenIdConnectRequest request, Worker user, AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManagerWorker.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIddictServerDefaults.AuthenticationScheme);

            if (!request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
            }
            var claimsIdentity = (ClaimsIdentity)ticket.Principal.Identity;
            claimsIdentity.AddClaim(new Claim("nivo_pristupa", "worker"));
            ticket.SetResources("resource_server");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }

    }
}
