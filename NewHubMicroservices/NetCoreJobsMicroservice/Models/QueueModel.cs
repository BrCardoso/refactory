using Commons;
using Commons.Base;
using Commons.Enums;
using System;
using System.Collections.Generic;
using static Commons.Base.Nit;

namespace NetCoreJobsMicroservice.Models
{
    #region Entrada
    public class QueueModel
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string aggregator { get; set; }
        public MovimentTypeEnum movementtype { get; set; }
        public string status { get; set; }
        public DateTime incdate { get; set; }
        public LinkInformation linkinformation { get; set; }
        public List<QueueBeneficiary> beneficiary { get; set; }
        public charge charge { get; set; }
    }       

    public class Transference
    {
        public string providerproductcode { get; set; }
        public string product { get; set; }
        public DateTime startdate { get; set; }
    }

    public partial class Return
    {
        public NitStatusTask status { get; set; }
        public string response { get; set; }
        public DateTime datetime { get; set; }
    }

    #endregion

    #region EntradaResponse

    public class ResponseQueue : Return
    {
        public Guid personguid { get; set; }
    }

    public class ResponseNIT
    {
        public Guid movementguid { get; set; }
        public List<ResponseQueue> Return { get; set; }
    }
    #endregion

    #region SaidaNIT
    

    #endregion

    #region SaidaNITResponse
    public class NITResponse {
        public string Message { get; set; }
        public NitModel Data { get; set; }
    }
    #endregion
}