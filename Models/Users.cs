using System;
using System.Collections.Generic;

namespace app.Models
{
    public partial class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PrimaryName { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTime JoinedDate { get; set; }
        public string AvatarLocation { get; set; }
    }
}
