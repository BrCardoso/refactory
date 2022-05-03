using System;
using System.Collections.Generic;
using Commons;
using System.Linq;
using Commons.Base;
using System.ComponentModel.DataAnnotations;

namespace BeneficiaryAppService.Models
{
    #region new
    public class FamilyFull : MethodFeedback
    {
        public Guid guid { get; set; }

        [NotEmpty]
        public Guid hubguid { get; set; }
        public Guid personguid { get; set; }
        [NotEmpty]
        public string aggregator { get; set; }
        public List<BeneficiaryIn> family { get; set; }
        internal void AddPerson(PersonDB person)
        {
            var ben = family.Where(b => b.personguid == person.Guid).FirstOrDefault();
            ben.Name = person.Name;
            ben.Cpf = person.Cpf;
            ben.Birthdate = person.Birthdate;
            ben.Gender = person.Gender;
            ben.Maritalstatus = person.Maritalstatus;
            ben.Mothername = person.Mothername;
            ben.Rg = person.Rg;
            ben.Issuingauthority = person.Issuingauthority;
            ben.complementaryinfos = person.complementaryinfos;
            ben.documents = person.documents;
            ben.Addresses = person.Addresses;
            ben.Phoneinfos = person.Phoneinfos;
            ben.emailinfos = person.emailinfos;
            ben.expeditiondate = person.expeditiondate;
        }

        public static explicit operator FamilyFull(FamilyDb v)
        {
            FamilyFull ret = new FamilyFull
            {
                hubguid = v.hubguid,
                guid = v.guid,
                personguid = v.personguid,
                aggregator = v.aggregator,
                family = new List<BeneficiaryIn>()
            };
            foreach (var item in v.family)
            {
                var ben = new BeneficiaryIn
                {
                    Kinship = item.Kinship,
                    personguid = item.personguid,
                    Benefitinfos = item.benefitinfos,
                    BlockDate = item.BlockDate,
                    BlockReason = item.BlockReason,
                    Origin = item.Origin,
                    Sequencial = item.Sequencial,
                    Typeuser = item.Typeuser
                };

                ret.family.Add(ben);
            }
            return ret;
        }
    }

    public class FamilyDb
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid personguid { get; set; }
        public string aggregator { get; set; }
        public string docType { get; set; }
        public List<BeneficiaryDb> family { get; set; }

        public static explicit operator FamilyDb(FamilyFull v)
        {
            FamilyDb ret = new FamilyDb
            {
                hubguid = v.hubguid,
                guid = v.guid,
                personguid = v.personguid,
                aggregator = v.aggregator,
                family = new List<BeneficiaryDb>()
            };

            foreach (var item in v.family)
            {
                var ben = new BeneficiaryDb
                {
                    Kinship = item.Kinship,
                    personguid = item.personguid,
                    benefitinfos = item.Benefitinfos,
                    BlockDate = item.BlockDate,
                    BlockReason = item.BlockReason,
                    Origin = item.Origin,
                    Sequencial = item.Sequencial,
                    Typeuser = item.Typeuser
                };

                ret.family.Add(ben);
            }
            return ret;
        }
    }

    public class BeneficiaryIn : Beneficiary
    {
        public Guid personguid { get; set; }

        public EmployeeinfoClean employeeinfo { get; set; }

        public static explicit operator BeneficiaryIn(BeneficiaryDb v)
        {
            BeneficiaryIn b = new BeneficiaryIn
            {
                personguid = v.personguid,
                Typeuser = v.Typeuser,
                Origin = v.Origin,
                Sequencial = v.Sequencial,
                Kinship = v.Kinship,
                BlockDate  = v.BlockDate,
                BlockReason  = v.BlockReason,
                Benefitinfos = v.benefitinfos
            };
            return b;
        }
    }

    public class BenefitDocument
    {
        [NotEmpty]
        public Guid guid { get; set; }
        [NotEmpty]
        public Guid hubguid { get; set; }
        [NotEmpty]
        public Guid personguid { get; set; }
        [NotEmpty]
        public string aggregator { get; set; }
        [NotEmpty]
        public List<Family> family { get; set; }
        [NotEmpty]
        public string file { get; set; }
        public string docType { get; set; }
    }
    
    public class BeneficiaryDb : BeneficiaryAddInfo
    {
        public Guid personguid { get; set; }

        #region BeneficiaryAddInfo
        public string Origin { get; set; }
        public string Sequencial { get; set; }
        public string Typeuser { get; set; }
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; }
        public List<Benefitinfo> benefitinfos { get; set; }
        #endregion
    }

    public class PersonDB : Person
    {
        public Guid Guid { get; set; }
        public string docType { get; set; }

        public static explicit operator PersonDB(QueueBeneficiary v)
        {
            return new PersonDB
            {
                Guid = v.personguid,
                Name = v.Name,
                Cpf = v.Cpf,
                Birthdate = v.Birthdate,
                Gender = v.Gender,
                Maritalstatus = v.Maritalstatus,
                Mothername = v.Mothername,
                Kinship = v.Kinship,
                Rg = v.Rg,
                Issuingauthority = v.Issuingauthority,
                complementaryinfos = v.complementaryinfos,
                documents = v.documents,
                Addresses = v.Addresses,
                Phoneinfos = v.Phoneinfos,
                emailinfos = v.emailinfos,
                expeditiondate = v.expeditiondate
            };
        }

        public static explicit operator PersonDB(BeneficiaryIn v)
        {
            PersonDB ret = new PersonDB
            {
                Guid = v.personguid,
                Name = v.Name,
                Cpf = v.Cpf,
                Birthdate = v.Birthdate,
                Gender = v.Gender,
                Maritalstatus = v.Maritalstatus,
                Mothername = v.Mothername,
                Kinship = v.Kinship,
                Rg = v.Rg,
                Issuingauthority = v.Issuingauthority,
                complementaryinfos = v.complementaryinfos,
                documents = v.documents,
                Addresses = v.Addresses,
                Phoneinfos = v.Phoneinfos,
                emailinfos = v.emailinfos,
                expeditiondate = v.expeditiondate
            };
            return ret;
        }

        public static explicit operator PersonDB(FamilyIn.beneficiary v)
        {
            PersonDB ret = new PersonDB
            {
                Guid = v.personguid,
                Name = v.Name,
                Cpf = v.Cpf,
                Birthdate = v.Birthdate,
                Gender = v.Gender,
                Maritalstatus = v.Maritalstatus,
                Mothername = v.Mothername,
                Rg = v.Rg,
                Kinship = v.Kinship,
                Issuingauthority = v.Issuingauthority,
                complementaryinfos = v.complementaryinfos,
                documents = v.documents,
                Addresses = v.Addresses,
                Phoneinfos = v.Phoneinfos,
                emailinfos = v.emailinfos,
                expeditiondate = v.expeditiondate
            };
            return ret;
        }
    }

    static class extentions
    {
        /// <summary>
        /// Compara 2 objetos do tipo person e retorna lista de diferenças
        /// </summary>
        /// <param name="val1">New person object</param>
        /// <param name="val2">Old person object</param>
        /// <returns></returns>
        public static List<Change> DetailedCompare(this PersonDB val1, PersonDB val2)
        {
            List<Change> variances = new List<Change>();
            var oType = new PersonDB().GetType();

            foreach (var oProperty in oType.GetProperties())
            {
                string[] fields = new string[] { "GENDER", "NAME", "CPF", "MARITALSTATUS", "MOTHERNAME", "RG", "ISSUINGAUTHORITY", "ADDRESSES" };
                if (!"guid,origin,doctype,kinship".Contains(oProperty.Name.ToLower()) &&
                    !(oProperty.PropertyType.IsGenericType && oProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    Change v = new Change();
                    v.Attribut = oProperty.Name;
                    v.Newvalue = oProperty.GetValue(val1, null);
                    v.Oldvalue = oProperty.GetValue(val2, null);
                    v.Date = DateTime.Now;
                    v.Sync = fields.Contains(oProperty.Name.ToUpper()) ? true : false;

                    if (v.Newvalue == null & v.Oldvalue != null)
                        variances.Add(v);
                    else if (v.Newvalue != null)
                        if (!v.Newvalue.Equals(v.Oldvalue))
                            variances.Add(v);
                }
                else if (oProperty.Name.ToLower() == "complementaryinfos" && val1.complementaryinfos != null)
                {
                    if (val1.complementaryinfos.Count > 0)
                    {
                        foreach (Complementaryinfo complementaryinfoVal1 in val1.complementaryinfos)
                        {
                            if (val2.complementaryinfos != null)
                            {
                                Complementaryinfo complementaryinfoVal2 = val2.complementaryinfos.Where(c => c.type == complementaryinfoVal1.type).FirstOrDefault();
                                if (complementaryinfoVal1?.value != complementaryinfoVal2?.value)
                                {
                                    Change v = new Change();
                                    v.Attribut = "complementaryinfos:type:" + complementaryinfoVal1?.type;
                                    v.Newvalue = complementaryinfoVal1?.value;
                                    v.Oldvalue = complementaryinfoVal2?.value;
                                    v.Date = DateTime.Now;
                                    v.Sync = false;
                                    //v.SyncDate = null;
                                    variances.Add(v);
                                }
                            }
                            else
                            {
                                Change v = new Change();
                                v.Attribut = "complementaryinfos:type:" + complementaryinfoVal1?.type;
                                v.Newvalue = complementaryinfoVal1?.value;
                                v.Oldvalue = null;
                                v.Date = DateTime.Now;
                                v.Sync = false;
                                //v.SyncDate = null;
                                variances.Add(v);
                            }
                        }
                    }
                }
                else if (oProperty.Name.ToLower() == "addresses" && val1.Addresses != null)
                {
                    if (val1.Addresses.Count > 0)
                    {
                        foreach (Address addressVal1 in val1.Addresses)
                        {
                            if (val2.Addresses != null)
                            {
                                Address addressesVal2 = val2.Addresses.Where(c => c.zipcode == addressVal1.zipcode).FirstOrDefault();
                                if (addressVal1?.zipcode != addressesVal2?.zipcode || addressVal1?.number != addressesVal2?.number)
                                {
                                    Change v = new Change();
                                    v.Attribut = "addresses:zipcode:" + addressVal1?.zipcode + ",number" + addressVal1?.zipcode;
                                    v.Newvalue = addressVal1?.zipcode;
                                    v.Oldvalue = addressesVal2?.zipcode;
                                    v.Date = DateTime.Now;
                                    v.Sync = false;
                                    //v.SyncDate = null;
                                    variances.Add(v);
                                }
                            }
                            else
                            {
                                Change v = new Change();
                                v.Attribut = "addresses:zipcode:" + addressVal1?.zipcode;
                                v.Newvalue = addressVal1?.zipcode;
                                v.Oldvalue = null;
                                v.Date = DateTime.Now;
                                v.Sync = false;
                                //v.SyncDate = null;
                                variances.Add(v);
                            }
                        }
                    }
                }
            }

            return variances;
        }

        /// <summary>
        /// Compara 2 objetos do tipo Address e retorna lista de diferenças
        /// </summary>
        /// <param name="val1">New person object</param>
        /// <param name="val2">Old person object</param>
        public static List<Change> DetailedCompare(this Address val1, Address val2)
        {
            List<Change> variances = new List<Change>();
            var oType = val1.GetType();

            foreach (var oProperty in oType.GetProperties())
            {
                if (oProperty.Name.ToLower() != "guid")
                {
                    Change v = new Change();
                    v.Attribut = "ADDRESSES";
                    v.Newvalue = oProperty.GetValue(val1, null);
                    v.Oldvalue = oProperty.GetValue(val2, null);
                    v.Date = DateTime.Now;
                    v.Sync = false;
                    //v.SyncDate = null;
                    if (v.Newvalue == null & v.Oldvalue != null)
                        variances.Add(v);
                    else if (v.Newvalue != null)
                        if (!v.Newvalue.Equals(v.Oldvalue))
                            variances.Add(v);
                }
            }
            return variances;
        }
    }

    #endregion

    public class FamilyIn
    {
        public Guid guid { get; set; }
        public Guid personguid { get; set; }
        public List<beneficiary> family { get; set; }

        public class beneficiary :Person{
            //benef
            public Guid personguid { get; set; }
            public EmployeeInfo employeeinfo { get; set; }
            public string Origin { get; set; }
            public string Sequencial { get; set; }
            [NotEmpty]
            public string Typeuser { get; set; }
            public DateTime? BlockDate { get; set; }
            public string BlockReason { get; set; }
            public List<Benefitinfo> Benefitinfos { get; set; }
                      
        }
    }

    public class FamilyOut : MethodFeedback
    {
        public Guid guid { get; set; }
    }
}