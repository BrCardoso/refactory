using NotifierAppService.Models;
using NotifierAppService.Repository.Interfaces;
using NotifierAppService.Services;
using NotifierAppService.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;

namespace NotifierAppService.Controllers
{
	[Route("api/v1/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly INotificationService _notificationService;

		public NotificationController(INotificationRepository notificationRepository, INotificationService notificationService)
		{
			_notificationRepository = notificationRepository;
			_notificationService = notificationService;
		}

		[HttpGet("all/{aggregator}/{hubGuid}")]
		public async Task<IActionResult> GetAllByUnityAsync([Required] string aggregator, [Required] string hubGuid)
		{
			return base.Ok(await _notificationRepository.FindAllNotificationsByAggregatorAsync(aggregator, hubGuid));
		}

		[HttpPost("Customer/{hubguid?}/aggregator/{aggregator}")]
		public async Task<IActionResult> AddNotificationTesteAsync([Required] Guid? hubguid, [Required] string aggregator, [FromBody] NotifierData newNotification)
		{
			if(ModelState.IsValid)
			{
				var result = await _notificationService.AddNotificationAsync(hubguid.Value, aggregator, newNotification);

				return result switch
				{
					NotificationStatus.Success => Ok(),
					NotificationStatus.EmptyOrNullFields => BadRequest("Deu ruim"),
					NotificationStatus.ErrorOnSave => BadRequest("nao salvou"),
					_ => BadRequest("Nao deu nada")
				};
			}

			return BadRequest();
		}
	}
}