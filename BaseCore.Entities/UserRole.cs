
using BaseCore.Common;
using BaseCore.Entities.Audit;
using System;
using System.Collections.Generic;

namespace BaseCore.Entities
{
    public partial class UserRole : Entity, IAuditable
    {
        public UserRole()
        {
            // Initialize navigation properties if needed
        }

        public Guid Guid { get; set; }

        public string UserId { get; set; }
        public string RoleId { get; set; }
        public bool IsActive { get; set; }
        public string? RoleUserId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; }
        public DateTime Modified { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation to the related Role
        public virtual Role Role { get; set; }

        // Navigation to the related User
        public virtual User User { get; set; }
    }
}
