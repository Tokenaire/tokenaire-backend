﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Tokenaire.Database;
using Tokenaire.Database.Models;

namespace tokenairebackend.Migrations
{
    [DbContext(typeof(TokenaireContext))]
    partial class TokenaireContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseEmail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Ip");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("Value")
                        .IsUnique()
                        .HasFilter("[Value] IS NOT NULL");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseIcoTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<string>("ICOBTCAddress");

                    b.Property<bool>("IsProcessed");

                    b.Property<bool?>("IsSuccessful");

                    b.Property<long>("OneAirePriceInSatoshies");

                    b.Property<string>("ProcessType");

                    b.Property<string>("RegisteredFromReferralLinkId");

                    b.Property<string>("TxIdSource");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.Property<long>("ValueReceivedInSatoshies");

                    b.Property<long>("ValueSentInAIRE");

                    b.HasKey("Id");

                    b.HasIndex("RegisteredFromReferralLinkId");

                    b.HasIndex("UserId");

                    b.HasIndex("TxIdSource", "ICOBTCAddress")
                        .IsUnique()
                        .HasFilter("[TxIdSource] IS NOT NULL AND [ICOBTCAddress] IS NOT NULL");

                    b.ToTable("ICOTransactions");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseIcoTransactionProcessHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<int>("IcoTransactionId");

                    b.HasKey("Id");

                    b.HasIndex("IcoTransactionId");

                    b.ToTable("ICOTransactionsHistory");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("Address")
                        .IsRequired();

                    b.Property<string>("ConcurrencyStamp");

                    b.Property<string>("Email");

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("EncryptedSeed")
                        .IsRequired();

                    b.Property<string>("ICOBTCAddress")
                        .IsRequired();

                    b.Property<string>("ICOBTCRefundAddress");

                    b.Property<DateTime?>("LastLoginDate");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail");

                    b.Property<string>("NormalizedUserName");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PublicKey")
                        .IsRequired();

                    b.Property<DateTime>("RegisteredDate");

                    b.Property<string>("RegisteredFromIP")
                        .IsRequired();

                    b.Property<string>("RegistrationInfoUserId");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("Signature")
                        .IsRequired();

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationInfoUserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUserReferralLink", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Type")
                        .IsRequired();

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserReferralLinks");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUserRegistrationInfo", b =>
                {
                    b.Property<string>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("RegisteredFromReferralLinkId");

                    b.HasKey("UserId");

                    b.HasIndex("RegisteredFromReferralLinkId");

                    b.ToTable("UserRegistrationInfos");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseIcoTransaction", b =>
                {
                    b.HasOne("Tokenaire.Database.Models.DatabaseUserReferralLink", "RegisteredFromReferralLink")
                        .WithMany()
                        .HasForeignKey("RegisteredFromReferralLinkId");

                    b.HasOne("Tokenaire.Database.Models.DatabaseUser", "user")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseIcoTransactionProcessHistory", b =>
                {
                    b.HasOne("Tokenaire.Database.Models.DatabaseIcoTransaction", "IcoTransaction")
                        .WithMany()
                        .HasForeignKey("IcoTransactionId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUser", b =>
                {
                    b.HasOne("Tokenaire.Database.Models.DatabaseUserRegistrationInfo")
                        .WithOne("User")
                        .HasForeignKey("Tokenaire.Database.Models.DatabaseUser", "Id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Tokenaire.Database.Models.DatabaseUserRegistrationInfo", "RegistrationInfo")
                        .WithMany()
                        .HasForeignKey("RegistrationInfoUserId");
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUserReferralLink", b =>
                {
                    b.HasOne("Tokenaire.Database.Models.DatabaseUser", "User")
                        .WithMany("ReferralLinks")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Tokenaire.Database.Models.DatabaseUserRegistrationInfo", b =>
                {
                    b.HasOne("Tokenaire.Database.Models.DatabaseUserReferralLink", "RegisteredFromReferralLink")
                        .WithMany()
                        .HasForeignKey("RegisteredFromReferralLinkId");
                });
#pragma warning restore 612, 618
        }
    }
}
