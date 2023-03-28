using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitHttpRequestExtensions
    {
        public static bool IsGet(this HttpRequest httpRequest) => httpRequest.Method == HttpMethod.Get.Method;
        public static bool IsPost(this HttpRequest httpRequest) => httpRequest.Method == HttpMethod.Post.Method;
    }
}
