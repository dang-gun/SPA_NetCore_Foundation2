﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModelsDB;

#nullable disable

namespace AspNetCore6React.Migrations.Mssql
{
    [DbContext(typeof(ModelsDbContext_Mssql))]
    [Migration("20220930092630_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

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
#pragma warning restore 612, 618
        }
    }
}
