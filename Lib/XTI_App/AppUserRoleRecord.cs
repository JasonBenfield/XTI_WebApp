﻿namespace XTI_App
{
    public sealed class AppUserRoleRecord
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Modifier { get; set; } = "";
    }
}
