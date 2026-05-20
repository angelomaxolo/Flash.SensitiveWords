using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.RestClient.Core;

namespace Flash.SensitiveWords.RestClient.Clients
{
    public class MessageFilterClient : ApiClientBase, IMessageFilterClient
    {
        public MessageFilterClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<FilterMessageResponse>?> FilterAsync(
            FilterMessageRequest request)
        {
            return PostAsync<FilterMessageRequest, ApiResponse<FilterMessageResponse>>(
                "sensitivewords/filter",
                request);
        }
    }
}
