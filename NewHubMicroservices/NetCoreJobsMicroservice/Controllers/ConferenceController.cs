using Commons;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interfaces;
using NetCoreJobsMicroservice.Services.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Controllers
{
	[Route("api/v1/[controller]")]
	public class ConferenceController : Controller
	{
		private readonly IConferenceRepository _conferenceRepository;
		private readonly IConferenceService _conferenceService;
		private readonly IConfiguration _config;

		public ConferenceController(IConferenceRepository conferenceRepository, IConferenceService conferenceService, IConfiguration configuration)
		{
			_conferenceRepository = conferenceRepository;
			_conferenceService = conferenceService;
			_config = configuration;
		}

		[HttpPost("Customer/{token}/CompareAndSave")]
		public async Task<IActionResult> CompareAndSaveValuesAsync(Guid token, [FromBody] ConferenceDB.Conference<ConferenceDB.Error> conference)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

			if (ModelState.IsValid)
			{
				try
				{
					if (conference.contracts.Count < 1)
						return BadRequest("É necessario informar pelo menos um contrato");

					if (conference.invoiceDetails.Any(x => x.errorList.Any()))
						return BadRequest("Nao é possivel realizar mais de uma comparaçao no mesmo batimento");

					if (await _conferenceRepository.FindDocByGuidAndDocTypeAsync(conference.guid, token, aggregator) is ConferenceDB.Conference<ConferenceDB.Error> currentInvoice)
						if (currentInvoice.status.ToUpper() == "FECHADO")
							return BadRequest("O batimento informado ja está fechado!");

					if (conference.guid == Guid.Empty)
					{
						conference.guid = Guid.NewGuid();
						conference.incdate = DateTime.Now;
					}

					var currentInvoiceByRefdate = await _conferenceRepository.FindByRefDateAsync(conference.hubguid, conference.aggregator, conference.providerguid, conference.refdate, conference.contracts);
					if (currentInvoiceByRefdate is ConferenceDB.Conference<ConferenceDB.Error>)
						if (currentInvoiceByRefdate.guid != conference.guid && currentInvoiceByRefdate.status != "RECUSADO")
							return Conflict("Ja existe um registro com o mesmo 'providerguid', 'refdate' e 'contracts' informado!");

					if (!(await _conferenceService.CompareAsync(conference, aggregator, Authorization) is ConferenceDB.Conference<ConferenceDB.Error> conferenceErrors))
						return BadRequest("Algo de errado aconteceu ao fazer as comparaçoes dos dados, tente novamente!");

					conference.docType = "InvoiceValidation";
					conferenceErrors.status = "PENDENTE";

					if (!(await _conferenceRepository.UpsertAsync(conference) is ConferenceDB.Conference<ConferenceDB.Error> createdConference))
						return BadRequest("Não foi possivel adicionar no banco.");

					return Ok(createdConference);
				}
				catch (Exception ex)
				{
					return Problem(ex.ToString());
				}
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpPost("Customer/{token}/Save")]
		public async Task<IActionResult> SaveAsync(Guid token, [FromBody] ConferenceDB.Conference<ConferenceDB.Error> conference)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

			if (ModelState.IsValid)
			{
				if (conference.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
				if (conference.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

				if (conference.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
				if (conference.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

				if (conference.contracts.Count < 1)
					return BadRequest("É necessario informar pelo menos um contrato");

				if (!(await _conferenceRepository.FindDocByGuidAndDocTypeAsync(conference.guid, token, conference.aggregator) is ConferenceDB.Conference<ConferenceDB.Error> currentInvoice))
					return NotFound("O documento informado nao existe no banco!");

				if (currentInvoice.status.ToUpper() != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE para realizar essa açao!");

				var currentInvoiceByRefdate = await _conferenceRepository.FindByRefDateAsync(conference.hubguid, conference.aggregator, conference.providerguid, conference.refdate, conference.contracts);

				if (currentInvoiceByRefdate is ConferenceDB.Conference<ConferenceDB.Error>)
					if (currentInvoiceByRefdate.guid != conference.guid)
						return Conflict("Ja existe outro registro com o mesmo 'refdate' e 'contracts' informado!");

				conference.docType = "InvoiceValidation";

				if (!(await _conferenceRepository.UpsertAsync(conference) is ConferenceDB.Conference<ConferenceDB.Error> createdConference))
					return BadRequest("Não foi possivel atualizar os dados no banco");

				return Ok(createdConference);
			}

			return BadRequest();
		}

		[HttpPost("Customer/{token}/Finalize")]
		public async Task<IActionResult> FinishAsync(Guid token, [FromBody] ConferenceDB.Conference<ConferenceDB.ErrorRequired> conference)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

			if (conference.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
			if (conference.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

			if (ModelState.IsValid)
			{
				if (conference.contracts.Count < 1)
					return BadRequest("É necessario informar pelo menos um contrato");

				if (!(await _conferenceRepository.FindDocByGuidAndDocTypeAsync(conference.guid, token, aggregator) is ConferenceDB.Conference<ConferenceDB.Error> currentInvoice))
					return NotFound("O documento informado nao existe no banco!");

				if (currentInvoice.status.ToUpper() != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE para realizar essa açao!");

				var currentInvoiceByRefdate = await _conferenceRepository.FindByRefDateAsync(conference.hubguid, conference.aggregator, conference.providerguid, conference.refdate, conference.contracts);
				if (currentInvoiceByRefdate is ConferenceDB.Conference<ConferenceDB.Error>)
					if (currentInvoiceByRefdate.guid != conference.guid)
						return Conflict("Ja existe um registro com o mesmo 'refdate' e 'contracts' informado!");

				conference.docType = "InvoiceValidation";
				conference.status = "FECHADO";

				if (!(await _conferenceService.FinishSolutionsAsync(conference, aggregator, Authorization) is ConferenceExtractDB.ConferenceExtract))
					return BadRequest("Nao foi possivel realizar o fechamento");

				if (!(await _conferenceRepository.UpsertAsync(conference) is ConferenceDB.Conference<ConferenceDB.ErrorRequired> finalizedInvoice))
					return BadRequest("Nao foi possivel salvar as informaçoes no banco, tente novamente!");

				return Ok(finalizedInvoice);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpGet("Customer/{token}")]
		public async Task<IActionResult> GetConferencesAsync(Guid token)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				if (!(await _conferenceRepository.FindAllInvoicesAsync(aggregator, token) is IEnumerable<ConferenceDB.Conference<ConferenceDB.Error>> conferences))
					return NotFound("Nao existe nenhuma conferencia com as informaçoes repassadas");

				return Ok(conferences);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpGet("Customer/{token}/Details/{guid}")]
		public async Task<IActionResult> GetConferenceDetailsAsync(Guid token, [Required] Guid? guid)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				if (!(await _conferenceRepository.FindInvoiceDetailsAsync(guid.Value, token, aggregator) is ConferenceDB.Conference<ConferenceDB.Error> conferenceDetails))
					return NotFound("Nao existe nenhuma conferencia com esse 'guid'");

				return Ok(conferenceDetails);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpPatch("Customer/{token}/Block/{guid?}")]
		public async Task<IActionResult> BlockConferenceAsync(Guid token, [Required] Guid? guid)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				ConferenceDB.Conference<ConferenceDB.Error> currentInvoice = await _conferenceRepository.FindDocByGuidAndDocTypeAsync(guid.Value, token, aggregator);
				if (currentInvoice is null)
					return NotFound("Nao existe nenhuma conferencia com esse 'guid'");

				if (currentInvoice.status.ToUpper() != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE para realizar essa açao!");

				currentInvoice.status = "RECUSADO";

				if (!(await _conferenceRepository.UpsertAsync(currentInvoice) is ConferenceDB.Conference<ConferenceDB.Error> updatedInvoice))
					return BadRequest("Nao foi possivel bloquear esse batimento!");

				return Ok(updatedInvoice);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpDelete("Customer/{token}/Delete/{guid?}")]
		public async Task<IActionResult> DeleteConferenceAsync(Guid token, [Required] Guid? guid)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				ConferenceDB.Conference<ConferenceDB.Error> currentInvoice = await _conferenceRepository.FindDocByGuidAndDocTypeAsync(guid.Value, token, aggregator);
				if (currentInvoice is null)
					return NotFound("Nao existe nenhuma conferencia com esse 'guid'");

				if (currentInvoice.status.ToUpper() != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE para realizar essa açao!");

				if (!await _conferenceRepository.DeleteByGuidAsync(guid.Value, token, aggregator))
					return BadRequest("Nao foi possivel excluir esse batimento");

				return NoContent();
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpPost("Customer/{token}/Extract")]
		public async Task<IActionResult> NewExtractAsync(Guid token, [FromBody] ConferenceExtractInput<OperationType> bIExtractInput)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

			if (bIExtractInput.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
			if (bIExtractInput.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

			if (ModelState.IsValid)
			{
				if (bIExtractInput.operation.value == 0)
					return BadRequest("O valor precisa ser maior que zero!");

				if (!(bIExtractInput.operation.type.ToUpper() == "CREDITO" || bIExtractInput.operation.type.ToUpper() == "DEBITO"))
					return BadRequest("O tipo da operaçao precisa ser 'CREDITO' ou 'DEBITO'");

				if (!(await _conferenceRepository.FindDocByGuidAndDocTypeAsync(bIExtractInput.operation.invoicevalidationguid, token, aggregator) is ConferenceDB.Conference<ConferenceDB.Error> conferenceCurrent))
					return NotFound("Nao existe nenhum batimento com o 'invoicevalidationguid' informado");

				if (conferenceCurrent.status != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE, para realizar essa açao!");

				var invoiceExtract = await _conferenceRepository.FindExtracsByProviderAsync(bIExtractInput.hubguid, bIExtractInput.aggregator, bIExtractInput.providerguid);
				var addedExtracts = _conferenceService.NewExtract(bIExtractInput, invoiceExtract);

				if (!(await _conferenceRepository.UpsertExtractAsync(addedExtracts) is ConferenceExtractDB.ConferenceExtract updatedExtractsDB))
					return BadRequest("Nao foi possivel atualizar os extratos da conta corrente no banco, tente novamente!");

				return Created("", updatedExtractsDB);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpPut("Customer/{token}/Extract/Use")]
		public async Task<IActionResult> UseExtractsAsync(Guid token, [FromBody] ConferenceExtractInput<Operation> bIExtractInput)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				if (bIExtractInput.operation.value == 0)
					return BadRequest("O valor precisa ser maior que zero!");

				if (!(await _conferenceRepository.FindDocByGuidAndDocTypeAsync(bIExtractInput.operation.invoicevalidationguid, token, aggregator) is ConferenceDB.Conference<ConferenceDB.Error> conferenceCurrent))
					return NotFound("Nao existe nenhum batimento com o 'invoicevalidationguid' informado");

				if (conferenceCurrent.status != "PENDENTE")
					return BadRequest("O batimento precisa estar PENDENTE, para realizar essa açao!");

				var invoiceExtract = await _conferenceRepository.FindExtracsByProviderAsync(bIExtractInput.hubguid, bIExtractInput.aggregator, bIExtractInput.providerguid);

				if (invoiceExtract is null)
					return BadRequest("Nao existe nenhum extrato ou qualquer tipo de credito/debito");

				if (Math.Abs(invoiceExtract.account.currentvalue) < Math.Abs(bIExtractInput.operation.value))
					return BadRequest("Nao é possivel usar o valor informado, pois nao possui saldo para isso");

				invoiceExtract = _conferenceService.UseExtracts(bIExtractInput, invoiceExtract, aggregator, Authorization);

				if (!(await _conferenceRepository.UpsertExtractAsync(invoiceExtract) is ConferenceExtractDB.ConferenceExtract updatedExtracts))
					return BadRequest("Algo deu errado ao tentar salvar as modificaçoes no banco, tente novamente");

				return Ok(updatedExtracts);
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}

		[HttpGet("Customer/{token}/Extracts/{providerguid?}")]
		public async Task<IActionResult> GetExtractsAsync(Guid token, [Required] Guid? providerguid)
		{
			string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
			var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
			if (!validateUser.Success)
				return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
			HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
			if (ModelState.IsValid)
			{
				var currentExtracts = await _conferenceRepository.FindExtracsByProviderAsync(token, aggregator, providerguid.Value);
				return currentExtracts switch
				{
					ConferenceExtractDB.ConferenceExtract _ => Ok(currentExtracts.account),
					_ => Ok()
				};
			}
			else
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return BadRequest(JsonConvert.SerializeObject(errors));
			}
		}
	}
}