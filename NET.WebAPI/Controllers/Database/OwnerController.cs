﻿using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using NET5.WebAPI.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace NET5.WebAPI.Controllers
{
    [Route("api/owner")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
		private ILoggerManager _logger;
		private IRepositoryWrapper _repository;
		private LinkGenerator _linkGenerator;

		public OwnerController(ILoggerManager logger,
			IRepositoryWrapper repository,
			LinkGenerator linkGenerator)
		{
			_logger = logger;
			_repository = repository;
			_linkGenerator = linkGenerator;
		}

		[HttpGet("Shaped")]
		public async Task<IActionResult> GetOwners2([FromQuery] OwnerParameters ownerParameters)
        {
			PagedList<ShapedEntity> data = await _repository.Owner.GetPagedAsync(
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

		[HttpGet]
		[ServiceFilter(typeof(ValidateMediaTypeAttribute))]
		public IActionResult GetOwners([FromQuery] OwnerParameters ownerParameters)
		{

			if (!ownerParameters.ValidYearRange)
			{
				return BadRequest("Max year of birth cannot be less than min year of birth");
			}

			PagedList<ShapedEntity> owners = _repository.Owner.GetOwners(ownerParameters);

			Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(owners.MetaData));

			_logger.LogInfo($"Returned {owners.MetaData.TotalCount} owners from database.");

			var shapedOwners = owners.Select(o => o.Entity).ToList();

			var mediaType = (MediaTypeHeaderValue)HttpContext.Items["AcceptHeaderMediaType"];

			if (!mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase))
			{
				return Ok(shapedOwners);
			}

			for (var index = 0; index < owners.Count; index++)
			{
				var ownerLinks = CreateLinksForOwner(owners[index].Id, ownerParameters.Fields);
				shapedOwners[index].Add("Links", ownerLinks);
			}

			var ownersWrapper = new LinkCollectionWrapper<Entity>(shapedOwners);

			return Ok(CreateLinksForOwners(ownersWrapper));
		}

		[HttpGet("JWT"), Authorize]
		public IActionResult GetOwnersCustom([FromQuery] OwnerParameters ownerParameters)
		{
			var owners = _repository.Owner.GetAll();
			return Ok(owners);
			
		}

		[HttpGet("{id}", Name = "OwnerById")]
		[ServiceFilter(typeof(ValidateMediaTypeAttribute))]
		public IActionResult GetOwnerById(Guid id, [FromQuery] string fields)
		{
			var owner = _repository.Owner.GetOwnerById(id, fields);

			if (owner.Id == Guid.Empty)
			{
				_logger.LogError($"Owner with id: {id}, hasn't been found in db.");
				return NotFound();
			}

			var mediaType = (MediaTypeHeaderValue)HttpContext.Items["AcceptHeaderMediaType"];

			if (!mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInfo($"Returned shaped owner with id: {id}");
				return Ok(owner.Entity);
			}

			owner.Entity.Add("Links", CreateLinksForOwner(owner.Id, fields));

			return Ok(owner.Entity);
		}

		[HttpPost]
		public IActionResult CreateOwner([FromBody] Owner owner)
		{
			if (owner == null)
			{
				_logger.LogError("Owner object sent from client is null.");
				return BadRequest("Owner object is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid owner object sent from client.");
				return BadRequest("Invalid model object");
			}

			_repository.Owner.CreateOwner(owner);
			_repository.Save();

			return CreatedAtRoute("OwnerById", new { id = owner.Id }, owner);
		}

		[HttpPut("{id}")]
		public IActionResult UpdateOwner(Guid id, [FromBody] Owner owner)
		{
			if (owner == null)
			{
				_logger.LogError("Owner object sent from client is null.");
				return BadRequest("Owner object is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid owner object sent from client.");
				return BadRequest("Invalid model object");
			}

			var dbOwner = _repository.Owner.GetOwnerById(id);
			if (dbOwner == null)
			{
				_logger.LogError($"Owner with id: {id}, hasn't been found in db.");
				return NotFound();
			}

			_repository.Owner.UpdateOwner(dbOwner, owner);
			_repository.Save();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteOwner(Guid id)
		{
			var owner = _repository.Owner.GetOwnerById(id);
			if (owner == null)
			{
				_logger.LogError($"Owner with id: {id}, hasn't been found in db.");
				return NotFound();
			}

			_repository.Owner.DeleteOwner(owner);
			_repository.Save();

			return NoContent();
		}

		private IEnumerable<Link> CreateLinksForOwner(Guid id, string fields = "")
		{
			var links = new List<Link>
			{
				new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(GetOwnerById), values: new { id, fields }),
				"self",
				"GET"),

				new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(DeleteOwner), values: new { id }),
				"delete_owner",
				"DELETE"),

				new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(UpdateOwner), values: new { id }),
				"update_owner",
				"PUT")
			};

			return links;
		}

		private LinkCollectionWrapper<Entity> CreateLinksForOwners(LinkCollectionWrapper<Entity> ownersWrapper)
		{
			ownersWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(GetOwners), values: new { }),
					"self",
					"GET"));

			return ownersWrapper;
		}
	}
}
