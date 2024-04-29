using AuthServer.Core.Dtos;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Exceptions;

namespace AuthServer.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : CustomBaseController
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
		{
			return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
		}

		[Authorize(AuthenticationSchemes ="Bearer")]
		[HttpGet]
		public async Task<IActionResult> GetUser()
		{
			return ActionResultInstance(await _userService.GetUserByNameAsync(HttpContext.User.Identity.Name));
		}
	}
}
