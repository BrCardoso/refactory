using Commons;
using ProvidersAppservice.Business.Interface;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvidersAppservice.Business
{
    public class PriceTableBusiness : IPriceTableBusiness
    {
        private readonly IPriceTableRepository _repo;
        public PriceTableBusiness(IPriceTableRepository repo)
        {
            _repo = repo;
        }

        public async Task<MethodFeedback> Upsert(PriceTableCB model)
        {
            MethodFeedback mf = new MethodFeedback();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                //se guid estiver vazio, busca na base com outros parametros
                var find = await _repo.Get(model.guid);
                if (find == null)
                    find = await _repo.GetByProduct(model.hubguid, model.aggregator, model.providerguid, model.providerproductcode);

                if (find == null)//add
                {
                    var a = _repo.Upsert(model);
                    dict.Add("AddPriceTable:" + model.providerproductcode, "Success:" + a.Success);
                }
                else //upd
                {
                    if (model.prices != null)
                    {
                        List<Commons.Base.PriceTable> prices = new List<Commons.Base.PriceTable>();
                        prices = find.prices;
                        foreach (var currPrice in model.prices)
                        {
                            //Gravar sempre o objeto que o front enviar
                            int foundPriceTable = find.prices.FindIndex(x => x.Name == currPrice.Name);

                            if (foundPriceTable >= 0)
                            {
                                prices[foundPriceTable] = currPrice;
                            }
                            else
                            {
                                prices.Add(currPrice);
                            }
                        }
                        _repo.MutatePrices(find.guid, prices);

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
