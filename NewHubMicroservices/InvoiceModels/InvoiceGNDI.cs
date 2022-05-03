using Commons;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetCoreJobsMicroservice.Models
{
    public class InvoiceGNDIItem 
    {
        [Name("Mes Competencia")]
        public string MesCompetencia { get; set; }

        [Name("Cod Contrato")]
        public string CodContrato { get; set; }

        [Name("CPF")]
        public string CPF { get; set; }

        [Name("Nome do beneficiario")]
        public string Nome { get; set; }

        [Name("Dt. Nasc.")]
        public string DtNasc { get; set; }

        [Name("Plano")]
        public string Plano { get; set; }

        [Name("Descricao Plano")]
        public string DescricaoPlano { get; set; }

        [Name("Valor Faturado")]
        public string ValorFaturado { get; set; }

        [Name("Nro. Carteirinha")]
        public string Carteirinha { get; set; }
                
    }
    public class InvoiceGNDIListResult : MethodFeedback
    {
        public List<InvoiceGNDIItem> _InvoiceGNDIItem { get; set; }

        public InvoiceGNDIListResult(string filePath)
        {
            TextReader reader = new StreamReader(Path.GetTempPath() + filePath);
            CsvConfiguration cc = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                IgnoreQuotes = true,
                MissingFieldFound = null,
                Delimiter = ";"
            };
            var csvReader = new CsvReader(reader, cc);
            csvReader.Read();
            //csvReader.Read();
            //csvReader.Read();
            try
            {
                var records = csvReader.GetRecords<InvoiceGNDIItem>();
                this._InvoiceGNDIItem = new List<InvoiceGNDIItem>();
                this._InvoiceGNDIItem = records.ToList();
            }
            catch (CsvHelper.MissingFieldException ex)
            {
                this.Message = ex.ToString();
                this.Success = false;
            }
            catch (CsvHelper.CsvHelperException ex)
            {
                this.Message = ex.ToString();
                this.Success = false;
            }
            catch (System.Exception ex)
            {
                this.Message = ex.ToString();
                this.Success = false;
            }
        }
    }
}
