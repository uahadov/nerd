using System.Security.Cryptography;

namespace Nard.Interaction
{
    public interface IDiceSource { int NextDie(); }

    /// <summary>Local hotseat prototype only. Online authority must supply server dice in its future part.</summary>
    public sealed class LocalDiceSource : IDiceSource
    {
        public int NextDie() => RandomNumberGenerator.GetInt32(1,7);
    }
}
