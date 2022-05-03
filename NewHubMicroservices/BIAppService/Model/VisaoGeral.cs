using System;
using System.Collections.Generic;

namespace BIAppService.Model
{
    public class VisaoGeral
    {
        public string grupo { get; set; }
        public string codgrupo { get; set; }
        public string empresa { get; set; }
        public string codempresa { get; set; }
        public string unidade { get; set; }
        public string cnpj { get; set; }
        public string segcode { get; set; }
        public string subsegcode { get; set; }

        public string beneficiario_nome { get; set; }
        public string beneficiario_id { get; set; }
        public string beneficiario_tipo { get; set; }
        public string beneficiario_cpf { get; set; }
        public string beneficiario_idade { get; set; }
        public string beneficiario_sexo { get; set; }
        public string beneficiario_cidade { get; set; }
        public string beneficiario_uf { get; set; }



        public string beneficio_provedor { get; set; }
        public Guid beneficio_provedorguid { get; set; }
        public string beneficio_produto { get; set; }
        public string beneficio_produtocode { get; set; }
        public string beneficio_contrato { get; set; }
        public string beneficio_bloqueio_motivo { get; set; }
        public DateTime? beneficio_bloqueio_data { get; set; }
        public string beneficio_incDate { get; set; }
        public string beneficio_preco { get; set; }
    }


    public class VisaoGeral2
    {
        public string grupo { get; set; }
        public string codgrupo { get; set; }
        public string empresa { get; set; }
        public string codempresa { get; set; }
        public string unidade { get; set; }
        public string cnpj { get; set; }
        public List<Beneficiarios> beneficiarios { get; set; }


        public class Beneficiarios
        {
            public string beneficiario_nome { get; set; }
            public string beneficiario_id { get; set; }
            public string beneficiario_tipo { get; set; }
            public string beneficiario_cpf { get; set; }
            public string beneficiario_idade { get; set; }
            public string beneficiario_sexo { get; set; }
            public string beneficiario_cidade { get; set; }
            public string beneficiario_uf { get; set; }
            public List<Beneficio> beneficios { get; set; }

            public class Beneficio
            {
                public string segcode { get; set; }
                public string subsegcode { get; set; }
                public string beneficio_provedor { get; set; }
                public Guid beneficio_provedorguid { get; set; }
                public string beneficio_produto { get; set; }
                public string beneficio_produtocode { get; set; }
                public string beneficio_contrato { get; set; }
                public string beneficio_bloqueio_motivo { get; set; }
                public DateTime? beneficio_bloqueio_data { get; set; }
                public string beneficio_incDate { get; set; }
                public string beneficio_preco { get; set; }
            }
        }
    }
}
