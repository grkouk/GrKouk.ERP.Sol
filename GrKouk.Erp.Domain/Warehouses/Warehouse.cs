using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.CashFlow;

namespace GrKouk.Erp.Domain.Warehouses;

public class Warehouse
{
    public int Id { get; set; }
    [MaxLength(20)]
    [Required]
    public string Code { get; set; }

    [MaxLength(200)]
    [Required]
    public string Name { get; set; }
    [DataType(DataType.Date)]
    public DateTime DateCreated { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateLastModified { get; set; }
    private ICollection<WarehouseCompanyMapping> _companyMappings;
    public ICollection<WarehouseCompanyMapping> CompanyMappings
    {
        get => _companyMappings ??= new List<WarehouseCompanyMapping>();
        set => _companyMappings = value;
    }
}