using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Transactors;

namespace GrKouk.Web.ERP.Mappings
{
    public static class TransactorMappings
    {
        /// <summary>New entity from the Create form. Company mappings and dates are set by the caller.</summary>
        public static Transactor ToEntity(this TransactorCreateDto dto) => new()
        {
            Code = dto.Code,
            TaxNumber = dto.TaxNumber,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Zip = dto.Zip,
            PhoneWork = dto.PhoneWork,
            PhoneMobile = dto.PhoneMobile,
            PhoneFax = dto.PhoneFax,
            EMail = dto.EMail,
            TransactorTypeId = dto.TransactorTypeId
        };

        /// <summary>Edit form model. SelectedCompanies is set by the caller.</summary>
        public static TransactorModifyDto ToModifyDto(this Transactor src) => new()
        {
            Id = src.Id,
            Code = src.Code,
            TaxNumber = src.TaxNumber,
            Name = src.Name,
            Address = src.Address,
            City = src.City,
            Zip = src.Zip,
            PhoneWork = src.PhoneWork,
            PhoneMobile = src.PhoneMobile,
            PhoneFax = src.PhoneFax,
            EMail = src.EMail,
            TransactorTypeId = src.TransactorTypeId,
            Timestamp = src.Timestamp
        };

        /// <summary>
        /// Copies the editable fields onto a loaded entity. Id, Timestamp, DateCreated and the
        /// company mappings are deliberately not touched.
        /// </summary>
        public static void ApplyTo(this TransactorModifyDto dto, Transactor entity)
        {
            entity.Code = dto.Code;
            entity.TaxNumber = dto.TaxNumber;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.Zip = dto.Zip;
            entity.PhoneWork = dto.PhoneWork;
            entity.PhoneMobile = dto.PhoneMobile;
            entity.PhoneFax = dto.PhoneFax;
            entity.EMail = dto.EMail;
            entity.TransactorTypeId = dto.TransactorTypeId;
        }

        /// <summary>Details view. Companies is set by the caller.</summary>
        public static TransactorDetailDto ToDetailDto(this Transactor src) => new()
        {
            Id = src.Id,
            Code = src.Code,
            TaxNumber = src.TaxNumber,
            Name = src.Name,
            TransactorTypeName = src.TransactorType?.Name,
            TransactorTypeCode = src.TransactorType?.Code
        };
    }
}
