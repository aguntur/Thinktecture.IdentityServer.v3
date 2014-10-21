﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class KeyHashingRefreshTokenStore : KeyHashingTransientDataRepository<RefreshToken>, IRefreshTokenStore
    {
        public KeyHashingRefreshTokenStore(IRefreshTokenStore inner)
            : base(inner)
        {
        }
    }
    public class KeyHashingAuthorizationCodeStore : KeyHashingTransientDataRepository<AuthorizationCode>, IAuthorizationCodeStore
    {
        public KeyHashingAuthorizationCodeStore(IAuthorizationCodeStore inner)
            : base(inner)
        {
        }
    }
    public class KeyHashingTokenHandleStore : KeyHashingTransientDataRepository<Token>, ITokenHandleStore
    {
        public KeyHashingTokenHandleStore(ITokenHandleStore inner)
            : base(inner)
        {
        }
    }

    public class KeyHashingTransientDataRepository<T> : ITransientDataRepository<T>
        where T : ITokenMetadata
    {
        HashAlgorithm hash;
        ITransientDataRepository<T> inner;

        public KeyHashingTransientDataRepository(ITransientDataRepository<T> inner)
            : this(Constants.DefaultHashAlgorithm, inner)
        {
        }

        public KeyHashingTransientDataRepository(string hashName, ITransientDataRepository<T> inner)
        {
            hash = HashAlgorithm.Create(hashName);
            this.inner = inner;
        }

        protected string Hash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hashedBytes = hash.ComputeHash(bytes);
            var hashedString = Thinktecture.IdentityModel.Base64Url.Encode(hashedBytes);
            return hashedString;
        }

        public Task StoreAsync(string key, T value)
        {
            return inner.StoreAsync(Hash(key), value);
        }

        public Task<T> GetAsync(string key)
        {
            return inner.GetAsync(Hash(key));
        }

        public Task RemoveAsync(string key)
        {
            return inner.RemoveAsync(Hash(key));
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            return inner.GetAllAsync(subject);
        }

        public Task RevokeAsync(string subject, string client)
        {
            return inner.RevokeAsync(subject, client);
        }
    }
}