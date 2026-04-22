using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    public sealed class CreatePasswordOtpRequestDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;
    }

    public sealed class CreatePasswordOtpRequestWrapper
    {
        [JsonPropertyName("data")]
        public CreatePasswordOtpRequestDto Data { get; set; } = null!;
    }

}
