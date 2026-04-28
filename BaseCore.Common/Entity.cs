using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseCore.Common
{
    public class Entity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [NotMapped]
        public DateTime CreatedDateTime { get; set; } = new DateTime();

        [NotMapped]
        public string CreatedUser { get; set; }
    }
}
