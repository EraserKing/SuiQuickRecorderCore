using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuiQuickRecorderCore.Models.DTOs
{
    public class DebtResponseDto
    {
        [JsonPropertyName("debtList")]
        public List<DebtItemDto> DebtList { get; set; }
    }

    public class DebtItemDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
