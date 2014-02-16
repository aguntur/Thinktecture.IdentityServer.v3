﻿using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoResponseGenerator
    {
        private IUserService _profile;
        private ICoreSettings _settings;
        private ILogger _logger;

        public UserInfoResponseGenerator(IUserService profile, ICoreSettings settings, ILogger logger)
        {
            _profile = profile;
            _settings = settings;
            _logger = logger;
        }

        public Dictionary<string, object> Process(ValidatedUserInfoRequest request)
        {
            var profileData = new Dictionary<string, object>();
            var requestedClaimTypes = GetRequestedClaimTypes(request.AccessToken);
            _logger.InformationFormat("Requested claim types: {0}", requestedClaimTypes.ToSpaceSeparatedString());

            var claims = _profile.GetProfileData(
                request.AccessToken.Claims.First(c => c.Type == Constants.ClaimTypes.Subject).Value, requestedClaimTypes);
            
            foreach (var claim in claims)
            {
                profileData.Add(claim.Type, claim.Value);
            }

            _logger.InformationFormat("Profile service returned to the following claim types: {0}", claims.Select(c => c.Type).ToSpaceSeparatedString());

            return profileData;
        }

        public IEnumerable<string> GetRequestedClaimTypes(Token accessToken)
        {
            var claims = new List<string>();

            var tokenScopes = accessToken.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope);
            if (tokenScopes == null)
            {
                _logger.Warning("No scopes found in access token. aborting.");
                return claims;
            }

            _logger.InformationFormat("Scopes in access token: {0}", tokenScopes);

            var scopes = tokenScopes.Value.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var scopeDetails = _settings.GetScopes();
            

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);
                
                if (scopeDetail != null)
                {
                    if (scopeDetail.IsOpenIdScope)
                    {
                        claims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return claims;
        }
    }
}