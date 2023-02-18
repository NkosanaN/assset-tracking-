﻿using Microsoft.AspNetCore.Identity;

namespace Domain
{
    /*
     * This model is design to hold User data 
     * A)  FK => TrackedItems
     */
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;

        public ICollection<UserPhoto> UserPhotos { get; set; } = new List<UserPhoto>();
        public ICollection<ItemEmployeeAssignment> ItemEmployeeAssignments { get; set; } = new List<ItemEmployeeAssignment>();
    }
}
