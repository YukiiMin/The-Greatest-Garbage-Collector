using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Auth
{
    public sealed class LocalLoginRequestWrapper
    {
        [JsonPropertyName("data")]
        public LocalLoginRequestDto Data { get; set; } = null!;
    }
}
