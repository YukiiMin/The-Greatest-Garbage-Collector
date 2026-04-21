using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    public sealed class VerifyEmailRequestWrapper
    {
        [JsonPropertyName("data")]
        public VerifyEmailRequestDto Data { get; set; } = null!;
    }
}
