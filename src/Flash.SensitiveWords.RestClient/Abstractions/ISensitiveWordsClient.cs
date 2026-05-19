using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flash.SensitiveWords.RestClient.Abstractions
{
    public interface ISensitiveWordsClient
    {
        Task<ApiResponse<List<SensitiveWordDto>>?> GetAllAsync();
        Task<ApiResponse<SensitiveWordDto>?> GetByIdAsync(Guid id);
        Task<ApiResponse<SensitiveWordDto>?> CreateAsync(CreateSensitiveWordRequest request);
        Task<ApiResponse<bool>?> DeleteAsync(Guid id);
        Task<ApiResponse<SensitiveWordDto>?> UpdateAsync(UpdateSensitiveWordRequest request);
    }
}
