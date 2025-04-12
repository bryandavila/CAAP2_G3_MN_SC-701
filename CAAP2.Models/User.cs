using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CAAP2.Models;

public partial class User
{
    public int UserID { get; set; }

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public bool IsPremium { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsEndUser { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
