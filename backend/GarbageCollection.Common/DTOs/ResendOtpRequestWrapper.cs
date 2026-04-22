using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    public sealed class ResendOtpRequestWrapper
    {
        [JsonPropertyName("data")]
        public ResendOtpRequestDto Data { get; set; } = null!;
    }
}
