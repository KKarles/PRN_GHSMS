﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Repository.Models;

public partial class GenderHealthcareDBContext : DbContext
{
    public GenderHealthcareDBContext()
    {
    }

    public GenderHealthcareDBContext(DbContextOptions<GenderHealthcareDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analyte> Analytes { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<ConsultantProfile> ConsultantProfiles { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<MenstrualCycle> MenstrualCycles { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<TestBooking> TestBookings { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TestResultDetail> TestResultDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

      public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=GenderHealthcareDB;User ID=sa;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analyte>(entity =>
        {
            entity.HasKey(e => e.AnalyteId).HasName("PK__Analytes__D434E8633157A619");

            entity.HasIndex(e => e.AnalyteName, "UQ__Analytes__0438DD08F12E5080").IsUnique();

            entity.Property(e => e.AnalyteId).HasColumnName("AnalyteID");
            entity.Property(e => e.AnalyteName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.DefaultReferenceRange).HasMaxLength(100);
            entity.Property(e => e.DefaultUnit).HasMaxLength(50);
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answers__D48250242020DF84");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnswerText).IsRequired();
            entity.Property(e => e.ConsultantId).HasColumnName("ConsultantID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Consultant).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ConsultantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answers__Consult__619B8048");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Answers__Questio__60A75C0F");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2A324385F");

            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentStatus)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ConsultantId).HasColumnName("ConsultantID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.MeetingUrl)
                .HasMaxLength(255)
                .HasColumnName("MeetingURL");
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");

            entity.HasOne(d => d.Consultant).WithMany(p => p.AppointmentConsultants)
                .HasForeignKey(d => d.ConsultantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Consu__4CA06362");

            entity.HasOne(d => d.Customer).WithMany(p => p.AppointmentCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Custo__4BAC3F29");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Sched__4D94879B");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__AA12603819A523D9");

            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__BlogPosts__Autho__30F848ED");
        });

        modelBuilder.Entity<ConsultantProfile>(entity =>
        {
            entity.HasKey(e => e.ConsultantId).HasName("PK__Consulta__E5B83F392B80EA2C");

            entity.Property(e => e.ConsultantId)
                .ValueGeneratedNever()
                .HasColumnName("ConsultantID");
            entity.Property(e => e.Specialization).HasMaxLength(255);

            entity.HasOne(d => d.Consultant).WithOne(p => p.ConsultantProfile)
                .HasForeignKey<ConsultantProfile>(d => d.ConsultantId)
                .HasConstraintName("FK__Consultan__Consu__33D4B598");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6476615BE");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.RelatedAppointmentId).HasColumnName("RelatedAppointmentID");
            entity.Property(e => e.RelatedServiceId).HasColumnName("RelatedServiceID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.RelatedAppointment).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.RelatedAppointmentId)
                .HasConstraintName("FK__Feedback__Relate__6754599E");

            entity.HasOne(d => d.RelatedService).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.RelatedServiceId)
                .HasConstraintName("FK__Feedback__Relate__66603565");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__UserID__656C112C");
        });

        modelBuilder.Entity<MenstrualCycle>(entity =>
        {
            entity.HasKey(e => e.CycleId).HasName("PK__Menstrua__077B24D928EB3CB2");

            entity.Property(e => e.CycleId).HasColumnName("CycleID");
            entity.Property(e => e.ExpectedCycleLength).HasDefaultValue(28);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.MenstrualCycles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Menstrual__UserI__59FA5E80");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CA9EEC5E0");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.IsAnonymous).HasDefaultValue(false);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Customer).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Questions__Custo__5DCAEF64");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A63B8E0E3");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160EB996869").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9C8A5B6939CB3BA4");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.ConsultantId).HasColumnName("ConsultantID");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity.HasOne(d => d.Consultant).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ConsultantId)
                .HasConstraintName("FK__Schedules__Consu__37A5467C");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EAF9E90F40");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.ServiceType)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasMany(d => d.Analytes).WithMany(p => p.Services)
                .UsingEntity<Dictionary<string, object>>(
                    "ServiceAnalyte",
                    r => r.HasOne<Analyte>().WithMany()
                        .HasForeignKey("AnalyteId")
                        .HasConstraintName("FK__ServiceAn__Analy__412EB0B6"),
                    l => l.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .HasConstraintName("FK__ServiceAn__Servi__403A8C7D"),
                    j =>
                    {
                        j.HasKey("ServiceId", "AnalyteId").HasName("PK__ServiceA__E858FE6C0BCCD4BA");
                        j.ToTable("ServiceAnalytes");
                        j.IndexerProperty<int>("ServiceId").HasColumnName("ServiceID");
                        j.IndexerProperty<int>("AnalyteId").HasColumnName("AnalyteID");
                    });
        });

        modelBuilder.Entity<TestBooking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__TestBook__73951ACDB06D3B30");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BookedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BookingStatus)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Customer).WithMany(p => p.TestBookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestBooki__Custo__46E78A0C");

            entity.HasOne(d => d.Service).WithMany(p => p.TestBookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestBooki__Servi__47DBAE45");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__976902287741BE7C");

            entity.HasIndex(e => e.BookingId, "UQ__TestResu__73951ACC8D68F9B2").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.IssuedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithOne(p => p.TestResult)
                .HasForeignKey<TestResult>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestResul__Booki__52593CB8");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.IssuedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestResul__Issue__534D60F1");
        });

        modelBuilder.Entity<TestResultDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__TestResu__135C314D4DEDDA76");

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.AnalyteName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Flag).HasMaxLength(50);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Result).WithMany(p => p.TestResultDetails)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("FK__TestResul__Resul__5629CD9C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC819B0A38");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105340D51F7B0").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Sex).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__UserRoles__RoleI__2E1BDC42"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserRoles__UserI__2D27B809"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF27604F520DC76B");
                        j.ToTable("UserRoles");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                        j.IndexerProperty<int>("RoleId").HasColumnName("RoleID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}