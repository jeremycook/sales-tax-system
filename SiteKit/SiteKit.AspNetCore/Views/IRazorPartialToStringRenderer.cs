using System.Threading.Tasks;

namespace SiteKit.AspNetCore.Views
{
    public interface IRazorPartialToStringRenderer
    {
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
    }
}
