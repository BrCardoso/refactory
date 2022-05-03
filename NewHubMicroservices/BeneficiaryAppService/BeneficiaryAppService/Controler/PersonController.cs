using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Repository.Interfaces;
using Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeneficiaryAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly ILogger<PersonController> _logger;
        private readonly IConfiguration _config;
        private readonly IPersonRepository _personRepository  ;

        public PersonController(ILogger<PersonController> logger, IConfiguration config,IPersonRepository personRepository )
        {
            _logger = logger;
            _config = config;
            _personRepository = personRepository;
        }

        [HttpGet]
        [Route("{token}")]
        public async Task<object> GetAsync(Guid token)
        {
            try
            {
                PersonDB result = await _personRepository.FindByPersonGuidAsync(token);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound();
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
        [Route("{token}/simplified")]
        public async Task<object> GetSimplifiedAsync(Guid token)
        {
            try
            {
                PersonDB result = await _personRepository.FindByPersonGuidAsync(token);
                if (result != null)
                {
                    return Ok(new PersonInfo
                    {
                        name = result.Name,
                        cpf = result.Cpf
                    });
                }
                else
                {
                    return NotFound();
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
        [Route("Cpf/{cpf}/BirthDate/{birthdate}")]
        public async System.Threading.Tasks.Task<object> GetAsync(string cpf, DateTime birthdate)
        {
            try
            {
                PersonDB result = await _personRepository.FindByPersonNameAsync(null, cpf, birthdate);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound();
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
        [HttpPost]
        public async Task<object> PostAsync([FromBody]PersonDB person)
        {
            try
            {
                //verifica se objeto está nulo
                if (person != null)
                {
                    if (person.Guid == Guid.Empty)
                    {
                        var personDB = await _personRepository.FindByPersonNameAsync(person.Name, person.Cpf, person.Birthdate);
                        if (personDB != null)
                            person.Guid = personDB.Guid;
                    }
                   
                    var Result = await _personRepository.AddAsync(person);
                    if (Result.Success)
                        return Ok(person);
                    else
                        return Problem(Result.Message);
                }
                else
                {
                    return Conflict("Request body not well formated");
                }
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        [Route("Documents")]
        public async System.Threading.Tasks.Task<object> PostAsync([FromBody] PersonDocs model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                var a = await _personRepository.UpSertDocumentsAsync(model.personguid, model.documents);

                return Ok(a);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        [Route("ComplementaryInfos")]
        public object Post([FromBody] PersonComplementaryinfos model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                var a = _personRepository.UpSertComplInfosAsync(model.personguid, model.complementaryinfos);

                return Ok(a);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

    }
}
