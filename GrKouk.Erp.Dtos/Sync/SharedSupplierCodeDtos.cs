using System;
using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.Sync;

// Supplier codes sync between shops. Mirror to the cash register's GrKoukOrg.Erp.Dtos copy.

/// <summary>A supplier code as the hub holds it.</summary>
public class SharedSupplierCodeDto
{
    public string SupplierVat { get; set; } = string.Empty;
    public int MatchKind { get; set; }
    public string MatchKey { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public decimal QuantityFactor { get; set; }
    public bool IsActive { get; set; }
    public int Source { get; set; }
    public string SupplierDescription { get; set; }
    public int Version { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; }
}

public class SupplierCodePullResponseDto
{
    public List<SharedSupplierCodeDto> Codes { get; set; } = new();

    /// <summary>Taken before reading the rows. The shop's next pull asks from here.</summary>
    public DateTime ServerTimestamp { get; set; }
}

/// <summary>A change a shop sends: the mapping, and the hub version it last saw.</summary>
public class SupplierCodePushDto
{
    public string SupplierVat { get; set; } = string.Empty;
    public int MatchKind { get; set; }
    public string MatchKey { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public decimal QuantityFactor { get; set; }
    public bool IsActive { get; set; }
    public int Source { get; set; }
    public string SupplierDescription { get; set; }

    /// <summary>The version this shop last saw. Null: it has never seen this key on the hub.</summary>
    public int? BaseVersion { get; set; }

    public DateTime ModifiedAt { get; set; }
}

public class SupplierCodePushRequestDto
{
    public string ShopId { get; set; } = string.Empty;
    public List<SupplierCodePushDto> Codes { get; set; } = new();
}

/// <summary>The answer for one pushed code, at the same position as in the request.</summary>
public class SupplierCodePushResultDto
{
    public bool Accepted { get; set; }

    /// <summary>The row cannot be stored (blank key, factor not above zero, ...). Not a conflict.</summary>
    public bool Invalid { get; set; }

    /// <summary>The hub's version of this key after the push.</summary>
    public int Version { get; set; }

    /// <summary>When refused: the hub's current mapping, which the shop has to weigh against its own.</summary>
    public SharedSupplierCodeDto Hub { get; set; }
}

public class SupplierCodePushResponseDto
{
    public List<SupplierCodePushResultDto> Results { get; set; } = new();
    public DateTime ServerTimestamp { get; set; }
}
