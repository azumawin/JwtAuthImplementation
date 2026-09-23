using System;
using System.Collections.Generic;

namespace JwtAuthImplementation.Infrastructure;

public partial class RefreshToken
{
    public byte[] TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
