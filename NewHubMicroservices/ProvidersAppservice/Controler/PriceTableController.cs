using System;
using System.Collections.Generic;
using Commons;
using Commons.Base;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProvidersAppservice.Business.Interface;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;

namespace ProvidersAppservice.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PriceTableController : ControllerBase
    {
        private readonly IPriceTableBusiness _priceTableBusiness;
        private readonly IPriceTableRepository _priceTableRepository;
        private readonly IConfiguration _config;

        public PriceTableController(IPriceTableBusiness priceTableBusiness, IPriceTableRepository priceTableRepository, IConfiguration config)
        {
            _priceTableBusiness = priceTableBusiness;
            _priceTableRepository = priceTableRepository;
            _config = config;
        }

        [HttpGet]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> Get(Guid token)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _priceTableRepository.Get(token, aggregator);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound("Nenhuma tabela de preços localizada.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Provider/{provider}")]
        public async System.Threading.Tasks.Task<object> Prices(Guid token, Guid provider)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _priceTableRepository.GetByProvider(token, aggregator, provider);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound("Nenhuma tabela de preços localizada.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Provider/{provider}/Product/{product}")]
        public async System.Threading.Tasks.Task<object> Prices(Guid token, Guid provider, string product)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _priceTableRepository.GetByProduct(token, aggregator, provider, product);
                if (result != null)
                    return Ok(new List<PriceTableCB> { result }); //TODO: substituir: return Ok(result)
                else
                    return NotFound("Nenhuma tabela de preços localizada.");

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Provider/{provider}/Product/{product}/SpotValue/{spotvalue}")]
        public async System.Threading.Tasks.Task<object> Prices(Guid token, Guid provider, string product, float spotvalue)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _priceTableRepository.GetByProductSpot(token, aggregator, provider, product, spotvalue);
                if (result != null)
                    return Ok(new List<PriceTableCB> { result }); //TODO: substituir: return Ok(result)
                else
                    return NotFound("Nenhuma tabela de preços localizada.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet] //api/v1/PriceTable/Customer/65734e6c-248f-4243-8732-cde9c52e94a2/Provider/b96b77c0-a1e4-459a-8cb6-6d2d6583c111/Product/TESTEMICHEL/TableName/Amil Blue I 2020/Salary/0.0/Age/40
        [Route("Customer/{token}/Provider/{provider}/Product/{product}/TableName/{tablename}/getRange")]
        public async System.Threading.Tasks.Task<object> RangesOfPrices(Guid token, Guid provider, string product, string tablename, float? salary, int? age)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _priceTableRepository.GetByName(token, aggregator, provider, product, tablename);
                if (result != null)
                {
                    var modelRange = new PriceTable.Ranges();
                    var cType = result.Type;
                    foreach (var range in result.Range)
                    {
                        float? nValor = 0;
                        if (cType.ToUpper() == "FAIXA ETÁRIA" || cType.ToUpper() == "FAIXA ETARIA")
                        {
                            if (age == null)
                                return Problem("Tabela de preços por faixa etária, parametro 'age' obrigatório.");
                            nValor = age;
                        }
                        else if (cType.ToUpper() == "FAIXA SALARIAL")
                        {
                            if (salary == null)
                                return Problem("Tabela de preços por faixa etária, parametro 'salary' obrigatório.");
                            nValor = salary;
                        }

                        if (cType.ToUpper() == "FIXO" || (range.Initialrange <= nValor && range.Finalrange >= nValor))
                        {
                            modelRange.Initialrange = range.Initialrange;
                            modelRange.Finalrange = range.Finalrange;
                            modelRange.Employeevalue = range.Employeevalue;
                            modelRange.Relativevalue = range.Relativevalue;
                            modelRange.Householdvalue = range.Householdvalue;
                            modelRange.Discounttype = range.Discounttype;
                            modelRange.Employeediscountvalue = range.Employeediscountvalue;
                            modelRange.Relativediscountvalue = range.Relativediscountvalue;
                            modelRange.Householddiscountvalue = range.Householddiscountvalue;
                        }
                    }
                    if (modelRange.Employeevalue > 0)
                    { return Ok(modelRange); }
                    else
                    { return NotFound(); }
                }
                else 
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet] //api/v1/PriceTable/Customer/65734e6c-248f-4243-8732-cde9c52e94a2/Provider/b96b77c0-a1e4-459a-8cb6-6d2d6583c111/Product/TESTEMICHEL/TableName/Amil Blue I 2020/Salary/0.0/Age/40
        [Route("Customer/{token}/Provider/{provider}/Product/{product}/TableName/{tablename}")]
        public async System.Threading.Tasks.Task<object> RangesOfPrices(Guid token, Guid provider, string product, string tablename)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
            try
            {
               var result = await _priceTableRepository.GetByName(token, aggregator, provider, product, tablename);
                if (result != null)
                    return Ok(new List<PriceTable> { result }); //TODO: substituir: return Ok(result)
                else
                    return NotFound("Nenhuma tabela de preços localizada.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }



        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody] PriceTableCB model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (model.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
            if (model.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");
            try
            {
                //restante vai aqui
                if (model == null)
                    return Conflict("Request body not well formated");
                else
                {
                    var retcompany = await _priceTableBusiness.Upsert(model);
                    return Ok(retcompany);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

    }
}
