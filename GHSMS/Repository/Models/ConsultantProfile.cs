﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class ConsultantProfile
{
    public int ConsultantId { get; set; }

    public string Qualifications { get; set; }

    public string Experience { get; set; }

    public string Specialization { get; set; }

    public virtual User Consultant { get; set; }
}