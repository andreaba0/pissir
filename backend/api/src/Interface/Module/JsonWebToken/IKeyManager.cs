using System.Security.Cryptography;

namespace Interface.Module.JsonWebToken
{
    public interface IKeyService
    {
        public RSAParameters? GetKey(string kid);
    }
}