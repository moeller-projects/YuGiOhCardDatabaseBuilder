using System.Threading.Tasks;

namespace YuGiOhDatabaseBuilderV2.Parser
{
    interface IParser<T>
    {
        Task<T> ParseAsync(string html);
    }
}
