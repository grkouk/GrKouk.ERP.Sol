using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    /// <summary>
    /// Παράμετροι ανα εταιρεία
    /// </summary>
    public class GlobalSettings
    {
        public int Id { get; set; }
        /// <summary>
        /// Default product production series Ids
        /// </summary>
        public int ProductProduceSeriesId { get; set; }
        public int RawMaterialConsumeSeriesId { get; set; }
        //-------------------------------------------------------
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
