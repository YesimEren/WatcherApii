﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WatcherApi.Classes
{
    public class Admin
    {

        public int Id { get; set; }
        [StringLength(10)]
        public string Username { get; set; }
        [StringLength(10)]
        public string Password { get; set; }
        public string Roles { get; set; }
    }
}
