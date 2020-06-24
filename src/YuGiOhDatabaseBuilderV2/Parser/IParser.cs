using System.Threading.Tasks;

namespace YuGiOhDatabaseBuilderV2.Parser
{
    public interface IParser<T>
    {
        Task<T> ParseAsync(string html);
    }
}
