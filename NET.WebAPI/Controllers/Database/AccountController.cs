﻿using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace NET5.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private IRepositoryWrapper _repository;

		public AccountController(
			IRepositoryWrapper repository)
		{
			_repository = repository;
		}

		[HttpGet("Shaped")]
		public async Task<IActionResult> GetOwners2([FromQuery] AccountParameters ownerParameters)
		{
			PagedList<ShapedEntity> data = await _repository.Account.GetPagedAsync(
				orderBy: ownerParameters.OrderBy,
				page: ownerParameters.PageNumber,
				pageSize: ownerParameters.PageSize,
				onlyFields: ownerParameters.Fields,
				includeProperties: ownerParameters.IncludeEntities,
				searchTerm: ownerParameters.SearchTerm,
				includeSearch: ownerParameters.IncludeSearch);

			Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
			Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(data.MetaData));
			var shapedData = data.Select(o => o.Entity).ToList();
			return Ok(shapedData);
		}
	}
}
