using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuiQuickRecorderWebAPI.Entities
{
    [PrimaryKey(nameof(Id))]
    public class Account
    {
        [Column("id")]
        public required string Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("alts")]
        public required string[] Alts { get; set; }
    }
}
