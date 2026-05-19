using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.RestClient.Core;

namespace Flash.SensitiveWords.RestClient.Clients
{
    public class SensitiveWordsClient : ApiClientBase, ISensitiveWordsClient
    {
        public SensitiveWordsClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<List<SensitiveWordDto>>?> GetAllAsync()
        {
            return GetAsync<ApiResponse<List<SensitiveWordDto>>>("sensitivewords");
        }

        public Task<ApiResponse<SensitiveWordDto>?> GetByIdAsync(Guid id)
        {
            return GetAsync<ApiResponse<SensitiveWordDto>>($"sensitivewords/{id}");
        }

        public Task<ApiResponse<SensitiveWordDto>?> CreateAsync(
            CreateSensitiveWordRequest request)
        {
            return PostAsync<CreateSensitiveWordRequest, ApiResponse<SensitiveWordDto>>(
                "sensitivewords",
                request);
        }

        public Task<ApiResponse<bool>?> DeleteAsync(Guid id)
        {
            return DeleteAsync<ApiResponse<bool>>($"sensitivewords/{id}");
        }
        public async Task<ApiResponse<SensitiveWordDto>?> UpdateAsync(UpdateSensitiveWordRequest request)
        {
           return await PutAsync<UpdateSensitiveWordRequest, ApiResponse<SensitiveWordDto>>(
                $"sensitivewords/{request.Id}",
                request);
        }
    }
}
