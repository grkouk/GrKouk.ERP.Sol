using System;

namespace GrKouk.Web.ERP.Helpers;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiryDate { get; set; }
    // Additional properties
    public DateTime CreatedDate { get; set; }
    public string CreatedByIp { get; set; }


}