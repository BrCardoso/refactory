using IdentityService.Business.Interfaces;
using IdentityService.Data.VOs.User;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IdentityService.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserBusiness _userBusiness;

		public UserController(IUserBusiness userBusiness)
		{
			_userBusiness = userBusiness;
		}

		/// <summary>
		/// Cria um novo usuario
		/// </summary>
		/// <param name="newUserData">Json contendo os dados do usuario a ser adicionado</param>
		/// <response code="201">Usuario adicionado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPost("Add")]
		[ProducesResponseType(typeof(UserVO), 201)]
		public async Task CreateUserAsync([FromBody] CreateUserVO newUserData)
		{
			if (ModelState.IsValid)
				await _userBusiness.CreateUserAsync(newUserData);
		}

		/// <summary>
		/// Traz as informaçoes de um usuario especifico.
		/// </summary>
		/// <param name="userGuid">Guid do usuario que deseja pegar as informaçoes</param>
		/// <response code="200">O dados foram consultados com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpGet("Find/{userGuid?}")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task GetUserAsync([Required] Guid? userGuid)
		{
			if (ModelState.IsValid)
				await _userBusiness.FindByUserGuidAsync(userGuid.Value);
		}

		/// <summary>
		/// Traz as informaçoes de todos os usuarios no banco.
		/// </summary>
		/// <response code="200">O dados foram consultados com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpGet("Find/All")]
		[ProducesResponseType(typeof(IEnumerable<UserVO>), 201)]
		public async Task GetUsersAsync()
		{
			await _userBusiness.FindAllUsersAsync();
		}

		/// <summary>
		/// Bloqueia um usuario especifico.
		/// </summary>
		/// <param name="userGuid">Guid do usuario que deseja bloquear</param>
		/// <response code="200">Usuario bloqueado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPatch("Block/{userGuid?}")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task BlockUserAsync([Required] Guid? userGuid)
		{
			if (ModelState.IsValid)
				await _userBusiness.BlockUserAsync(userGuid.Value);
		}

		/// <summary>
		/// Desbloqueia um usuario especifico.
		/// </summary>
		/// <param name="userGuid">Guid do usuario que deseja desbloquear</param>
		/// <response code="200">Usuario desbloqueado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPatch("Unblock/{userGuid?}")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task UnblockUserAsync([Required] Guid? userGuid)
		{
			if (ModelState.IsValid)
				await _userBusiness.UnblockUserAsync(userGuid.Value);
		}

		/// <summary>
		/// Atualiza qualquer informaçao do usuario.
		/// </summary>
		/// <param name="newUserData">Informaçoes do usuario que deseja atualizar</param>
		/// <response code="200">Usuario desbloqueado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPut("Update")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task UpdateUserAsync([FromBody] UpdateUserVO newUserData)
		{
			if (ModelState.IsValid)
				await _userBusiness.UpdateUserData(newUserData);
		}

		/// <summary>
		/// Remove um usuario do banco.
		/// </summary>
		/// <param name="userGuid">Guid do usuario que deseja deletar.</param>
		/// <response code="204">Usuario removido com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpDelete("Remove/{userGuid?}")]
		public async Task DeleteUserAsync([Required] Guid? userGuid)
		{
			if (ModelState.IsValid)
				await _userBusiness.DeleteUserAsync(userGuid.Value);
		}
	}
}