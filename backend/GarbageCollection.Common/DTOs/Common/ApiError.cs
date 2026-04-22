using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Common
{
    public sealed class ApiError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;
    }
}
