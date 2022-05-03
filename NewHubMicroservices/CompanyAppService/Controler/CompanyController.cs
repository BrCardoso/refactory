using System;
using System.Linq;
using System.Threading.Tasks;
using CompanyAppservice;
using CompanyAppservice.Models;
using CompanyAppService.Repository.Interface;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CompanyAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly ICompanyRepository _repository;

        public CompanyController(ILogger<CompanyController> logger,ICompanyRepository companyRepository)
        {
            _logger = logger;
            _repository = companyRepository;
        }

        [HttpGet]
        [Route("{token}")]
        public async Task<object> GetAsync(Guid token)
        {           
            try
            {
                var result = await _repository.GetCompanyByGUIDAsync(token);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return Problem("Nenhuma empresa localizada.");
                }
            }
            catch (Exception ex)
            {
                //em caso de falha
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());

            }
        }


        [HttpGet]
        [Route("Id/{companyid}")]
        public async Task<object> GetAsync(string companyid)
        {
            try
            {
                var result = await _repository.GetCompanyByCNPJAsync(companyid);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return Problem("Nenhuma empresa localizada.");
                }
            }
            catch (Exception ex)
            {
                //em caso de falha
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());

            }
        }

        [HttpGet]
        [Route("CompanyId/{companyid}")]
        public async Task<object> GetCompanyAsync(string companyid)
        {
            try
            {
                try
                {
                    var result = await _repository.GetCompanyByCNPJAsync(companyid);
                    if (result != null)
                    {
                        return Ok(result.SingleOrDefault().guid.ToString());
                    }
                    else
                    {
                        return Problem("Nenhuma empresa localizada.");
                    }
                }
                catch (Exception ex)
                {
                    //em caso de falha
                    _logger.LogInformation(ex.ToString(), ex);
                    return BadRequest(ex.ToString());

                }
            }
            catch (Exception ex)
            {
                //em caso de falha
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());

            }
        }

        // POST: api/Person
        public async Task<object> PostAsync([FromBody]CompanyCB company)
        {
            try
            {
                //valida objeto
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }


                var retcompany = await _repository.PostAsync(company);
                return Ok(retcompany);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }
    }
}
