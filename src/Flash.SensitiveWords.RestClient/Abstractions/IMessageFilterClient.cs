using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flash.SensitiveWords.RestClient.Abstractions
{
    public interface IMessageFilterClient
    {
        Task<ApiResponse<FilterMessageResponse>?> FilterAsync(FilterMessageRequest request);
    }
}
