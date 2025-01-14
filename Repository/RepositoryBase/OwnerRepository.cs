﻿using Contracts;
using Entities;
using Entities.Extensions;
using Entities.Models;
using System;
using System.Linq;

namespace Repository.Contracts
{
    public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
    {
        public OwnerRepository(RepositoryContext repositoryContext) : base(repositoryContext) { }

        public PagedList<ShapedEntity> GetOwners(OwnerParameters ownerParameters)
        {
            var owners = GetAll().Where(o => o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
                                        o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth);

            SearchByName(owners.AsQueryable(), ownerParameters.Name);

            var sortedOwners = ApplySort(owners.AsQueryable(), ownerParameters.OrderBy);
            var shapedOwners = ShapeData(sortedOwners, ownerParameters.Fields);

            return PagedList<ShapedEntity>.ToPagedList(shapedOwners,
                ownerParameters.PageNumber,
                ownerParameters.PageSize);
        }

        private void SearchByName(IQueryable<Owner> owners, string ownerName)
        {
            if (!owners.Any() || string.IsNullOrWhiteSpace(ownerName))
                return;

            if (string.IsNullOrEmpty(ownerName))
                return;

            owners = owners.Where(o => o.Name.ToLowerInvariant().Contains(ownerName.Trim().ToLowerInvariant()));
        }

        public ShapedEntity GetOwnerById(Guid ownerId, string fields)
        {
            var owner = GetAll().Where(owner => owner.Id.Equals(ownerId))
                .DefaultIfEmpty(new Owner())
                .FirstOrDefault();

            return ShapeDataSingle(owner, fields);
        }

        public Owner GetOwnerById(Guid ownerId)
        {
            return GetAll().Where(owner => owner.Id.Equals(ownerId))
                .DefaultIfEmpty(new Owner())
                .FirstOrDefault();
        }

        public void CreateOwner(Owner owner)
        {
            Insert(owner);
        }

        public void UpdateOwner(Owner dbOwner, Owner owner)
        {
            dbOwner.Map(owner);
            Update(dbOwner);
        }

        public void DeleteOwner(Owner owner)
        {
            Delete(owner);
        }
    }
}
