using System.Security.Cryptography;

namespace Uinsure.Api.Policies;

public interface IPolicyReferenceGenerator
{
    string Create();
}

public sealed class PolicyReferenceGenerator : IPolicyReferenceGenerator
{
    public string Create() => $"POL-{RandomNumberGenerator.GetHexString(12)}";
}
