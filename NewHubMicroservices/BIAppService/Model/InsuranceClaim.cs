using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIAppService.Model
{
    public class InsuranceClaim
    {
        public string grupo { get; set; }
        public string codgrupo { get; set; }
        public string empresa { get; set; }
        public string codempresa { get; set; }
        public string unidade { get; set; }
        public string cnpj { get; set; }
        public string segcode { get; set; }
        public string provedor { get; set; }
        public string beneficiario_nome { get; set; }
        public string beneficiario_cpf { get; set; }
        public string beneficiario_motivo_bloqueio { get; set; }
        public string evento_data { get; set; }
        public string evento_tipo { get; set; }
        public string evento_valor_pago { get; set; }
        public string evento_competencia { get; set; }
    }
}
