using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Auth.Local
{

    public sealed class LocalRegisterRequestDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;
    }

    public sealed class LocalRegisterRequestWrapper
    {
        [JsonPropertyName("data")]
        public LocalRegisterRequestDto Data { get; set; } = null!;
    }
}
