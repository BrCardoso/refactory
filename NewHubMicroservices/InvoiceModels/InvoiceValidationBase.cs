using Commons;
using NetCoreJobsMicroservice.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetCoreJobsMicroservice.Models
{
    public class InvoiceValidationBase
    {
        public string NAME { get; set; }

        public string DOCUMENT { get; set; }

        public string PRODUCTCODE { get; set; }

        public string PRODUCTDESCRIPTION { get; set; }

        public string CONTRACT { get; set; }

        public float VALUE { get; set; }

        public string CARDNUMBER { get; set; }

        public DateTime BIRTHDATE { get; set; }

        public string REFDATE { get; set; }

        public string TYPEUSER { get; set; }
        public DateTime BLOCKDATE { get; set; }
        public string BLOCKMOTIVE { get; set; }

    }
    public class InvoiceValidationBaseResult : MethodFeedback
    {
        public List<InvoiceValidationBase> _InvoiceValidationBase { get; set; }

        #region METHODS
        public InvoiceValidationBaseResult(ProviderGuid provider,string filePath) {
            if (provider.Value == ProviderGuid.GNDI.Value)
            {
                this._InvoiceValidationBase = ((InvoiceValidationBaseResult)new InvoiceGNDIListResult(filePath))._InvoiceValidationBase;
            }
            else if (provider.Value == ProviderGuid.AMIL.Value)
            {
                ///do something
                ///var ret = (InvoiceValidationBaseResult)new %invoice result do provedor%(filePath);
            }
            else
            {
                this.Success = false;
                this.Message = "MAPEAMENTO NÃO ENCONTRADO";
            }
        }
        private InvoiceValidationBaseResult()
        {
            
        }

        /// <summary>
        /// Transforma objeto do arquivi GNDI em modelo base do comparativo do bate-fatura
        /// </summary>
        /// <param name="v">objeto</param>
        public static explicit operator InvoiceValidationBaseResult(InvoiceGNDIListResult v)
        {
            InvoiceValidationBaseResult ret = new InvoiceValidationBaseResult();
            try
            {
                ret._InvoiceValidationBase = new List<InvoiceValidationBase>();
                foreach (InvoiceGNDIItem item in v._InvoiceGNDIItem)
                {
                    DateTime datanascimento;
                    DateTime.TryParseExact(item.DtNasc, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out datanascimento);
                    ret._InvoiceValidationBase.Add(new InvoiceValidationBase()
                    {
                        NAME = item.Nome,
                        DOCUMENT = item.CPF,
                        PRODUCTCODE = item.Plano,
                        PRODUCTDESCRIPTION = item.DescricaoPlano,
                        CONTRACT = item.CodContrato,
                        VALUE = (float)item.ValorFaturado.newToFloat(),
                        CARDNUMBER = item.Carteirinha.Replace(".", "").Trim(),
                        BIRTHDATE = datanascimento,
                        REFDATE = item.MesCompetencia
                    });
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.ToString();
            }            

            return ret;
        }
        #endregion

        
    }

    public static class Help {
        public static float? newToFloat(this String floatString)
        {
            float? ret;
            try
            {
                ret = float.Parse(floatString.Replace(",", "."), CultureInfo.InvariantCulture);

            }
            catch (Exception)
            {
                ret = null;
            }

            return ret;
        }
    }
}