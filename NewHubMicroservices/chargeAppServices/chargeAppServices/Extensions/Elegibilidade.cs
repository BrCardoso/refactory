using chargeAppServices.Models;

namespace ChargeAppServices.Extensions
{
    public static class Elegibilidade
    {
        public static bool Verifica(EmployeeInfo oEmployeeInfo, productruleFood oGeneralRules)
        {
            bool lReturn = false;
            if (oGeneralRules.items != null)
            {
                foreach (var oItem in oGeneralRules.items)
                {   //Cargo, Departamento, Centro de Custo, Funcao, Sindicato, Categoria Funcional
                    //Cargo, Departamento, Centro de Custo, Profissao, Sindicato, Categoria Funcional
                    if (oGeneralRules.elegibilitylimitations.ToUpper() == "CARGO")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Role.ToUpper())
                        { lReturn = true; }
                    }
                    else if (oGeneralRules.elegibilitylimitations.ToUpper() == "DEPARTAMENTO")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Department.ToUpper())
                        { lReturn = true; }
                    }
                    else if (oGeneralRules.elegibilitylimitations.ToUpper() == "CENTRO DE CUSTO")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Costcenter.ToUpper())
                        { lReturn = true; }
                    }
                    else if (oGeneralRules.elegibilitylimitations.ToUpper() == "PROFISSAO" || oGeneralRules.elegibilitylimitations.ToUpper() == "PROFISSÃO")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Occupation.ToUpper())
                        { lReturn = true; }
                    }
                    else if (oGeneralRules.elegibilitylimitations.ToUpper() == "SINDICATO")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Union.ToUpper())
                        { lReturn = true; }
                    }
                    else if (oGeneralRules.elegibilitylimitations.ToUpper() == "CATEGORIA FUNCIONAL")
                    {
                        if (oItem.description.ToUpper() == oEmployeeInfo.Functionalcategory.ToUpper())
                        { lReturn = true; }
                    }
                }
            }
            return lReturn;
        }
    }
}
