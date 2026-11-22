using MiracleShoesDesktop.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiracleShoesDesktop.Services
{
    public static class Session
    {
        public static User? CurrentUser { get; set; }
        public static bool IsAuthorized => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.IdUserRole == 1;
        public static bool IsManager => CurrentUser?.IdUserRole == 2;
        public static bool IsUser => CurrentUser?.IdUserRole == 3;
    }
}
