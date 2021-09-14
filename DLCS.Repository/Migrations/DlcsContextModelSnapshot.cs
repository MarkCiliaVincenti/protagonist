﻿// <auto-generated />
using System;
using DLCS.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DLCS.Repository.Migrations
{
    [DbContext(typeof(DlcsContext))]
    partial class DlcsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasPostgresExtension("tablefunc")
                .HasAnnotation("Relational:Collation", "en_US.UTF-8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.HasSequence("batch_id_sequence")
                .StartsAt(570185L)
                .HasMin(1L)
                .HasMax(9223372036854775807L);

            modelBuilder.Entity("DLCS.Model.Assets.Asset", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Batch")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<long>("Duration")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("0");

                    b.Property<string>("Error")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasDefaultValueSql("NULL::character varying");

                    b.Property<char>("Family")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("character(1)")
                        .HasDefaultValueSql("'I'::\"char\"");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Height")
                        .HasColumnType("integer");

                    b.Property<string>("ImageOptimisationPolicy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasDefaultValueSql("'fast-lossy'::character varying");

                    b.Property<bool>("Ingesting")
                        .HasColumnType("boolean");

                    b.Property<int>("MaxUnauthorised")
                        .HasColumnType("integer");

                    b.Property<string>("MediaType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasDefaultValueSql("'image/jp2'::character varying");

                    b.Property<int>("NumberReference1")
                        .HasColumnType("integer");

                    b.Property<int>("NumberReference2")
                        .HasColumnType("integer");

                    b.Property<int>("NumberReference3")
                        .HasColumnType("integer");

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("PreservedUri")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Reference1")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Reference2")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Reference3")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<int>("Space")
                        .HasColumnType("integer");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("ThumbnailPolicy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasDefaultValueSql("'original'::character varying");

                    b.Property<int>("Width")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Batch" }, "IX_ImagesByBatch");

                    b.HasIndex(new[] { "Id", "Customer", "Space" }, "IX_ImagesByCustomerSpace");

                    b.HasIndex(new[] { "Id", "Customer", "Error", "Batch" }, "IX_ImagesByErrors")
                        .HasFilter("((\"Error\" IS NOT NULL) AND ((\"Error\")::text <> ''::text))");

                    b.HasIndex(new[] { "Reference1" }, "IX_ImagesByReference1");

                    b.HasIndex(new[] { "Reference2" }, "IX_ImagesByReference2");

                    b.HasIndex(new[] { "Reference3" }, "IX_ImagesByReference3");

                    b.HasIndex(new[] { "Customer", "Space" }, "IX_ImagesBySpace");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("DLCS.Model.Assets.ImageLocation", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Nas")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("S3")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.ToTable("ImageLocation");
                });

            modelBuilder.Entity("DLCS.Model.Assets.ThumbnailPolicy", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Sizes")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("Id");

                    b.ToTable("ThumbnailPolicies");
                });

            modelBuilder.Entity("DLCS.Model.Customers.Customer", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<bool>("AcceptedAgreement")
                        .HasColumnType("boolean");

                    b.Property<bool>("Administrator")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Keys")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("DLCS.Model.Customers.CustomerOriginStrategy", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Credentials")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<bool>("Optimised")
                        .HasColumnType("boolean");

                    b.Property<string>("Regex")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Strategy")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.ToTable("CustomerOriginStrategies");
                });

            modelBuilder.Entity("DLCS.Model.Customers.SignupLink", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("SignupLinks");
                });

            modelBuilder.Entity("DLCS.Model.Security.AuthService", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("CallToAction")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("ChildAuthService")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<string>("Label")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<string>("PageDescription")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<string>("PageLabel")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Profile")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("RoleProvider")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Ttl")
                        .HasColumnType("integer")
                        .HasColumnName("TTL");

                    b.HasKey("Id", "Customer");

                    b.ToTable("AuthServices");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.ActivityGroup", b =>
                {
                    b.Property<string>("Group")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Inhabitant")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime?>("Since")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Group");

                    b.ToTable("ActivityGroups");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.AuthToken", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("BearerToken")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("CookieId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastChecked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionUserId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("Ttl")
                        .HasColumnType("integer")
                        .HasColumnName("TTL");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "BearerToken" }, "IX_AuthTokens_BearerToken");

                    b.HasIndex(new[] { "CookieId" }, "IX_AuthTokens_CookieId");

                    b.ToTable("AuthTokens");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.Batch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValueSql("nextval('batch_id_sequence'::regclass)");

                    b.Property<int>("Completed")
                        .HasColumnType("integer");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<int>("Errors")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Submitted")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Superseded")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Customer", "Superseded", "Submitted" }, "IX_BatchTest");

                    b.ToTable("Batches");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.CustomHeader", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Role")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasDefaultValueSql("NULL::character varying");

                    b.Property<int?>("Space")
                        .HasColumnType("integer");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Customer", "Space" }, "IX_CustomHeaders_ByCustomerSpace");

                    b.ToTable("CustomHeaders");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.CustomerImageServer", b =>
                {
                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("ImageServer")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Customer");

                    b.ToTable("CustomerImageServers");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.CustomerStorage", b =>
                {
                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<int>("Space")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("LastCalculated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("NumberOfStoredImages")
                        .HasColumnType("bigint");

                    b.Property<string>("StoragePolicy")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<long>("TotalSizeOfStoredImages")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalSizeOfThumbnails")
                        .HasColumnType("bigint");

                    b.HasKey("Customer", "Space");

                    b.ToTable("CustomerStorage");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.EntityCounter", b =>
                {
                    b.Property<string>("Type")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Scope")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<long>("Next")
                        .HasColumnType("bigint");

                    b.HasKey("Type", "Scope", "Customer");

                    b.ToTable("EntityCounters");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.ImageOptimisationPolicy", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("TechnicalDetails")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("Id");

                    b.ToTable("ImageOptimisationPolicies");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.ImageServer", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("InfoJsonTemplate")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.ToTable("ImageServers");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.ImageStorage", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<int>("Space")
                        .HasColumnType("integer");

                    b.Property<bool>("CheckingInProgress")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastChecked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<long>("ThumbnailSize")
                        .HasColumnType("bigint");

                    b.HasKey("Id", "Customer", "Space");

                    b.HasIndex(new[] { "Customer", "Space", "Id" }, "IX_ImageStorageByCustomerSpace");

                    b.ToTable("ImageStorage");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.InfoJsonTemplate", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Template")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.HasKey("Id");

                    b.ToTable("InfoJsonTemplates");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.MetricThreshold", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Metric")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<long?>("Lower")
                        .HasColumnType("bigint");

                    b.Property<long?>("Upper")
                        .HasColumnType("bigint");

                    b.HasKey("Name", "Metric");

                    b.ToTable("MetricThresholds");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.NamedQuery", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<bool>("Global")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Template")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("Id");

                    b.ToTable("NamedQueries");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.OriginStrategy", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("RequiresCredentials")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("OriginStrategies");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.Queue", b =>
                {
                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasDefaultValueSql("'default'::character varying");

                    b.Property<int>("Size")
                        .HasColumnType("integer");

                    b.HasKey("Customer")
                        .HasName("Queues_pkey");

                    b.ToTable("Queues");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.Role", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("Aliases")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("AuthService")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id", "Customer");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.RoleProvider", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("AuthService")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Configuration")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<string>("Credentials")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("RoleProviders");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.SessionUser", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Roles")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.HasKey("Id");

                    b.ToTable("SessionUsers");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.Space", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImageBucket")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("Keep")
                        .HasColumnType("boolean");

                    b.Property<int>("MaxUnauthorised")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<bool>("Transform")
                        .HasColumnType("boolean");

                    b.HasKey("Id", "Customer")
                        .HasName("Spaces_pkey");

                    b.ToTable("Spaces");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.StoragePolicy", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<long>("MaximumNumberOfStoredImages")
                        .HasColumnType("bigint");

                    b.Property<long>("MaximumTotalSizeOfStoredImages")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("StoragePolicies");
                });

            modelBuilder.Entity("DLCS.Repository.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Customer")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
