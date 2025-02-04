﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("owner")]
    public class Owner
    {
        [Key]
        [Column("OwnerId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(60, ErrorMessage = "Name can't be longer than 60 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(100, ErrorMessage = "Address can not be loner then 100 characters")]
        public string Address { get; set; }
        public List<Account> Accounts { get; set; }
    }

    public class OwnerParameters : QueryStringParameters
    {
        // Sorting by default
        public OwnerParameters()
        {
            OrderBy = "name";
        }

        // Filtering
        public uint MinYearOfBirth { get; set; }
        public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;
        public bool ValidYearRange => MaxYearOfBirth > MinYearOfBirth;
        // Searching
        public string Name { get; set; }
    }
}
