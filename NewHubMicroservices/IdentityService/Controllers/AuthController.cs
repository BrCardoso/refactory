using IdentityService.Business.Interfaces;
using IdentityService.Data.VOs.Auth;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IdentityService.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthBusiness _authBusiness;

		public AuthController(IAuthBusiness authBusiness)
		{
			_authBusiness = authBusiness;
		}

		/// <summary>
		/// Autentica e gera o JWT de 1h do usuario solicitado
		/// </summary>
		/// <param name="signIn">Email e senha para a autenticaçao</param>
		/// <response code="200">Usuario autenticado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPost("SignIn")]
		[ProducesResponseType(typeof(TokenVO), 200)]
		public async Task AuthAsync([FromBody] SignInVO signIn)
		{
			if (ModelState.IsValid)
				await _authBusiness.SignInAsync(signIn);
		}

		/// <summary>
		/// Determina se determinado token é valido
		/// </summary>
		/// <param name="accessToken">Guid de acesso do token a ser validado</param>
		/// <response code="200">Token validado com sucesso!</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpGet("Validate/{accessToken?}")]
		[ProducesResponseType(typeof(TokenVO), 200)]
		public async Task ValidateTokenAsync([Required] Guid? accessToken)
		{
			if (ModelState.IsValid)
				await _authBusiness.ValidateToken(accessToken.Value);
		}

		/// <summary>
		/// Atualiza determinado token
		/// </summary>
		/// <param name="refreshToken">Guid do refresh token que deseja exterder</param>
		/// <response code="200">Token atualizado com sucesso</response>
		/// <response code="400">Ocorreu algum problema na solicitaçao!</response>
		/// <response code="500">Ocorreu um erro interno no servidor, contacte os desenvolvedores</response>
		[HttpPost("Refresh/{refreshToken?}")]
		[ProducesResponseType(typeof(TokenVO), 200)]
		public async Task RefreshTokenAsync([Required] Guid? refreshToken)
		{
			if (ModelState.IsValid)
				await _authBusiness.RefreshTokenAsycn(refreshToken.Value);
		}
	}
}