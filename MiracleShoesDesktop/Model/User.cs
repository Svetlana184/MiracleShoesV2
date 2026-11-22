using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiracleShoesDesktop.Model;

public partial class User
{
    public int IdUser { get; set; }

    public string FirstName { get; set; } = null!;

    public string SecondName { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Login { get; set; } = null!;

    public int IdUserRole { get; set; }

    public virtual UserRole IdUserRoleNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [NotMapped]
    public string FullName =>
        $"{Surname} {FirstName} {SecondName}".Trim();
}
