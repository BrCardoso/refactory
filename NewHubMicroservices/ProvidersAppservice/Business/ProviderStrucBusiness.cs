using Commons;
using ProvidersAppservice.Business.Interface;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProvidersAppservice.Business
{
    public class ProviderStrucBusiness : IProviderStrucBusiness
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IProviderStrucRepository _providerStrucRepository;
        public ProviderStrucBusiness(IProviderRepository providerRepository, IProviderStrucRepository providerStrucRepository)
        {
            _providerRepository = providerRepository;
            _providerStrucRepository = providerStrucRepository;
        }

        public async Task<ProviderCustomerCB> GetProductNames(ProviderCustomerCB model)
        {
            if (model != null)
            {
                var provider = await _providerRepository.Get(model.Providerguid);
                if (provider != null)
                {
                    foreach (var item in model.Products)
                    {
                        item.Description = provider.Products.Where(x => x.Providerproductcode == item.Providerproductcode).SingleOrDefault().Description;
                    }
                }
            }
            return model;
        }

        public async Task<MethodFeedback> UpsertAsync(ProviderStrucCB model)
        {
            MethodFeedback mf = new MethodFeedback();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                //se guid estiver vazio, busca na base com outros parametros
                var obj = await _providerStrucRepository.Get(model.Guid);
                if (obj == null)
                    obj = await _providerStrucRepository.GetByProviderGuid(model.Hubguid, model.Aggregator, model.Providerguid);
                
                if (obj == null)//se não achar, atribui novo guid para cadastro novo
                {
                    _providerStrucRepository.Upsert(model);
                }
                else
                {
                    var find = Converter.ProviderCustomerCBToProviderStrucCB.Convert(obj);
                    if (model.accesscredentials != null)
                        _providerStrucRepository.MutateCredentials(model.Guid, model.accesscredentials);

                    if (model.Emailinfos != null)
                    {
                        var emailinfos = new List<Emailinfo>();
                        emailinfos = find.Emailinfos;

                        foreach (var email in model.Emailinfos)
                        {
                            if (string.IsNullOrEmpty(email.type))
                            {
                                if (emailinfos.Count > 1)
                                {
                                    emailinfos[0] = email;
                                }
                                else
                                {
                                    emailinfos.Add(email);
                                }
                            }
                            else
                            {
                                int index = find.Emailinfos.FindIndex(x => x.type == email.type);
                                 if (index < 0)
                                 {
                                    //inclui email
                                     emailinfos.Add(email);
                                 }
                                 else
                                 {
                                     emailinfos[index] = email;
                                 }
                            }
                        }
                        var ret = _providerStrucRepository.MutateEmails(find.Guid, emailinfos);
                    }

                    if (model.Products != null)
                    {
                        List<ProviderStrucCB.ProductCB> products = new List<ProviderStrucCB.ProductCB>();
                        products = find.Products;
                        foreach (var currProd in model.Products)
                        {
                            int index = find.Products.FindIndex(x => x.Providerproductcode == currProd.Providerproductcode);
                            if (index < 0)
                            {//inclui produto
                                products.Add(currProd);
                            }
                            else
                            {
                                products[index] = currProd;
                            }
                        }
                        var ret = _providerStrucRepository.MutateProducts(find.Guid, products);
                    }
                }

            }
            catch (Exception ex)
            {
                mf.Success = false;
                mf.Exception = true;
                mf.Message = ex.ToString();
            }
            mf.obj = dict;
            return mf;
        }
    }
}
