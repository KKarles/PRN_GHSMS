﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public string ServiceType { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<TestBooking> TestBookings { get; set; } = new List<TestBooking>();

    public virtual ICollection<Analyte> Analytes { get; set; } = new List<Analyte>();
}