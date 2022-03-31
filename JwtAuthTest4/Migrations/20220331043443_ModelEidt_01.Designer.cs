﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModelsDB;

#nullable disable

namespace JwtAuthTest4.Migrations
{
    [DbContext(typeof(ModelsDbContext))]
    [Migration("20220331043443_ModelEidt_01")]
    partial class ModelEidt_01
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("ModelsDB.User", b =>
                {
                    b.Property<int>("idUser")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SignName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("idUser");

                    b.ToTable("User");

                    b.HasData(
                        new
                        {
                            idUser = 1,
                            PasswordHash = "1111",
                            SignName = "test01"
                        },
                        new
                        {
                            idUser = 2,
                            PasswordHash = "1111",
                            SignName = "test02"
                        });
                });

            modelBuilder.Entity("ModelsDB.UserRefreshToken", b =>
                {
                    b.Property<int>("idUserRefreshToken")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ExpiresTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpCreated")
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RevokeTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("idUser")
                        .HasColumnType("INTEGER");

                    b.HasKey("idUserRefreshToken");

                    b.ToTable("UserRefreshToken");
                });
#pragma warning restore 612, 618
        }
    }
}
