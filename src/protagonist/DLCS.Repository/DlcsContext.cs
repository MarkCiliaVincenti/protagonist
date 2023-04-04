﻿

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using DLCS.Core.Enum;
using DLCS.Core.Types;
using DLCS.Model.Assets;
using DLCS.Model.Assets.CustomHeaders;
using DLCS.Model.Assets.NamedQueries;
using DLCS.Model.Auth.Entities;
using DLCS.Model.Customers;
using DLCS.Model.Policies;
using DLCS.Model.Processing;
using DLCS.Model.Spaces;
using DLCS.Model.Storage;
using DLCS.Repository.Auth;
using DLCS.Repository.Entities;
using DLCS.Repository.Serialisation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OriginStrategy = DLCS.Model.Policies.OriginStrategy;
namespace DLCS.Repository;

public partial class DlcsContext : DbContext
{
    // Deliverator version of DLCS stores serialized values in some db records 
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
        Converters = new List<JsonConverter> { new SessionUserRoleSerialiser() }
    };

    public DlcsContext()
    {
    }

    public DlcsContext(DbContextOptions<DlcsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityGroup> ActivityGroups { get; set; }
    public virtual DbSet<AuthService> AuthServices { get; set; }
    public virtual DbSet<AuthToken> AuthTokens { get; set; }
    public virtual DbSet<Batch> Batches { get; set; }
    public virtual DbSet<CustomHeader> CustomHeaders { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerImageServer> CustomerImageServers { get; set; }
    public virtual DbSet<CustomerOriginStrategy> CustomerOriginStrategies { get; set; }
    public virtual DbSet<CustomerStorage> CustomerStorages { get; set; }
    public virtual DbSet<EntityCounter> EntityCounters { get; set; }
    public virtual DbSet<Asset> Images { get; set; }
    public virtual DbSet<ImageLocation> ImageLocations { get; set; }
    public virtual DbSet<ImageOptimisationPolicy> ImageOptimisationPolicies { get; set; }
    public virtual DbSet<ImageServer> ImageServers { get; set; }
    public virtual DbSet<ImageStorage> ImageStorages { get; set; }
    public virtual DbSet<InfoJsonTemplate> InfoJsonTemplates { get; set; }
    public virtual DbSet<MetricThreshold> MetricThresholds { get; set; }
    public virtual DbSet<NamedQuery> NamedQueries { get; set; }
    public virtual DbSet<OriginStrategy> OriginStrategies { get; set; }
    public virtual DbSet<Queue> Queues { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleProvider> RoleProviders { get; set; }
    public virtual DbSet<SessionUser> SessionUsers { get; set; }
    public virtual DbSet<Space> Spaces { get; set; }
    public virtual DbSet<StoragePolicy> StoragePolicies { get; set; }
    public virtual DbSet<ThumbnailPolicy> ThumbnailPolicies { get; set; }
    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<SignupLink> SignupLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*
         * NOTE - the following was auto-generated by running the following command
         * dotnet ef dbcontext scaffold 
         */ 
        
        // Required to avoid errors like
        // The property 'Customer.Keys' is a collection or enumeration type with a value converter but with no value comparer.
        // Set a value comparer to ensure the collection/enumeration elements are compared correctly.
        var stringArrayComparer = new ValueComparer<string[]>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray());
        
        modelBuilder.HasPostgresExtension("tablefunc")
            .HasAnnotation("Relational:Collation", "en_US.UTF-8");

        modelBuilder.HasSequence<long>("batch_id_sequence")
            .StartsAt(570185)
            .IncrementsBy(1)
            .HasMin(1)
            .HasMax(9223372036854775807);

        modelBuilder.Entity<ActivityGroup>(entity =>
        {
            entity.HasKey(e => e.Group);

            entity.Property(e => e.Group).HasMaxLength(100);

            entity.Property(e => e.Inhabitant).HasMaxLength(500);

            entity.Property(e => e.Since).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<AuthService>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Customer });

            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.CallToAction).HasMaxLength(1000);

            entity.Property(e => e.ChildAuthService).HasMaxLength(500);

            entity.Property(e => e.Description).HasMaxLength(4000);

            entity.Property(e => e.Label).HasMaxLength(1000);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.PageDescription).HasMaxLength(4000);

            entity.Property(e => e.PageLabel).HasMaxLength(1000);

            entity.Property(e => e.Profile)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.RoleProvider).HasMaxLength(500);

            entity.Property(e => e.Ttl).HasColumnName("TTL");
        });

        modelBuilder.Entity<AuthToken>(entity =>
        {
            entity.HasIndex(e => e.BearerToken, "IX_AuthTokens_BearerToken");

            entity.HasIndex(e => e.CookieId, "IX_AuthTokens_CookieId");

            entity.Property(e => e.Id).HasMaxLength(100);

            entity.Property(e => e.BearerToken)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CookieId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Created).HasColumnType("timestamp with time zone");

            entity.Property(e => e.Expires).HasColumnType("timestamp with time zone");

            entity.Property(e => e.LastChecked).HasColumnType("timestamp with time zone");

            entity.Property(e => e.SessionUserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Ttl).HasColumnName("TTL");

            entity.Ignore(e => e.SessionUser);
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasIndex(e => new { e.Customer, e.Superseded, e.Submitted }, "IX_BatchTest");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('batch_id_sequence'::regclass)");

            entity.Property(e => e.Finished).HasColumnType("timestamp with time zone");

            entity.Property(e => e.Submitted).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<CustomHeader>(entity =>
        {
            entity.HasIndex(e => new { e.Customer, e.Space }, "IX_CustomHeaders_ByCustomerSpace");

            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Role)
                .HasMaxLength(500)
                .HasDefaultValueSql("NULL::character varying");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Created).HasColumnType("timestamp with time zone");

            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Keys)
                .IsRequired()
                .HasMaxLength(1000)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    stringArrayComparer);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<CustomerImageServer>(entity =>
        {
            entity.HasKey(e => e.Customer);

            entity.Property(e => e.Customer).ValueGeneratedNever();

            entity.Property(e => e.ImageServer)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<CustomerOriginStrategy>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Credentials)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Regex)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Strategy)
                .IsRequired()
                .HasMaxLength(500)
                .HasConversion(
                    v => v.GetDescription(),
                    v => v.GetEnumFromString<OriginStrategyType>(true));
        });

        modelBuilder.Entity<CustomerStorage>(entity =>
        {
            entity.HasKey(e => new { e.Customer, e.Space });

            entity.ToTable("CustomerStorage");

            entity.Property(e => e.LastCalculated).HasColumnType("timestamp with time zone");

            entity.Property(e => e.StoragePolicy).HasMaxLength(500);
        });

        modelBuilder.Entity<EntityCounter>(entity =>
        {
            entity.HasKey(e => new { e.Type, e.Scope, e.Customer });

            entity.Property(e => e.Type).HasMaxLength(100);

            entity.Property(e => e.Scope).HasMaxLength(100);
        });

        modelBuilder.Entity<Asset>().ToTable("Images");
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(e => e.Batch, "IX_ImagesByBatch");

            entity.HasIndex(e => new { e.Id, e.Customer, e.Space }, "IX_ImagesByCustomerSpace");

            entity.HasIndex(e => new { e.Id, e.Customer, e.Error, e.Batch }, "IX_ImagesByErrors")
                .HasFilter("((\"Error\" IS NOT NULL) AND ((\"Error\")::text <> ''::text))");

            entity.HasIndex(e => e.Reference1, "IX_ImagesByReference1");

            entity.HasIndex(e => e.Reference2, "IX_ImagesByReference2");

            entity.HasIndex(e => e.Reference3, "IX_ImagesByReference3");

            entity.HasIndex(e => new { e.Customer, e.Space }, "IX_ImagesBySpace");

            entity.Property(e => e.Id)
                .HasMaxLength(500)
                .HasConversion(
                    aId => aId.ToString(),
                    id => AssetId.FromString(id));

            entity.Property(e => e.Created)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.Duration)
                .IsRequired()
                .HasDefaultValueSql("0");

            entity.Property(e => e.Error)
                .HasMaxLength(1000)
                .IsRequired()
                .HasDefaultValueSql("NULL::character varying");

            entity.Property(e => e.Family)
                .IsRequired()
                .HasColumnType("char(1)")
                .HasDefaultValueSql("'I'::\"char\"")
                .HasConversion(
                    v => (char)v,
                    v => (AssetFamily)v);

            entity.Property(e => e.Finished).HasColumnType("timestamp with time zone");

            entity.Property(e => e.ImageOptimisationPolicy)
                .IsRequired()
                .HasMaxLength(500)
                .HasDefaultValueSql("'fast-lossy'::character varying");

            entity.Property(e => e.MediaType)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValueSql("'image/jp2'::character varying");

            entity.Property(e => e.Origin)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.PreservedUri)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Reference1)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Reference2)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Reference3)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Roles)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Tags)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.ThumbnailPolicy)
                .IsRequired()
                .HasMaxLength(500)
                .HasDefaultValueSql("'original'::character varying");

            entity.Property(e => e.MaxUnauthorised).IsRequired();
            entity.Property(e => e.Width).IsRequired();
            entity.Property(e => e.Height).IsRequired();
            entity.Property(e => e.NumberReference1).IsRequired();
            entity.Property(e => e.NumberReference2).IsRequired();
            entity.Property(e => e.NumberReference3).IsRequired();
            entity.Property(e => e.Ingesting).IsRequired();
            entity.Property(e => e.Batch).IsRequired();
            entity.Property(e => e.DeliveryChannels)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(
                    dc => string.Join(',', dc.OrderBy(v => v)),
                    dc => dc.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    stringArrayComparer);

            entity.Ignore(e => e.InitialOrigin);
        });

        modelBuilder.Entity<ImageLocation>(entity =>
        {
            entity.ToTable("ImageLocation");

            entity.Property(e => e.Id)
                .HasMaxLength(500)
                .HasConversion(
                    aId => aId.ToString(),
                    id => AssetId.FromString(id));;

            entity.Property(e => e.Nas)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.S3)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<ImageOptimisationPolicy>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Customer });
            
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TechnicalDetails)
                .IsRequired()
                .HasMaxLength(1000)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    stringArrayComparer);
        });

        modelBuilder.Entity<ImageServer>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.InfoJsonTemplate).HasMaxLength(500);
        });

        modelBuilder.Entity<ImageStorage>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Customer, e.Space });

            entity.ToTable("ImageStorage");

            entity.HasIndex(e => new { e.Customer, e.Space, e.Id }, "IX_ImageStorageByCustomerSpace");

            entity.Property(e => e.Id)
                .HasMaxLength(500)
                .HasConversion(
                    aId => aId.ToString(),
                    id => AssetId.FromString(id));;

            entity.Property(e => e.LastChecked).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<InfoJsonTemplate>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Template)
                .IsRequired()
                .HasMaxLength(4000);
        });

        modelBuilder.Entity<MetricThreshold>(entity =>
        {
            entity.HasKey(e => new { e.Name, e.Metric });

            entity.Property(e => e.Name).HasMaxLength(500);

            entity.Property(e => e.Metric).HasMaxLength(500);
        });

        modelBuilder.Entity<NamedQuery>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Template)
                .IsRequired()
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<OriginStrategy>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);
        });

        modelBuilder.Entity<Queue>(entity =>
        {
            entity.HasKey(e => new { e.Customer, e.Name });

            entity.Property(e => e.Customer).ValueGeneratedNever();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500)
                .HasDefaultValueSql("'default'::character varying");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Customer });

            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Aliases)
                .HasMaxLength(1000);

            entity.Property(e => e.AuthService)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<RoleProvider>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.AuthService)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Configuration).HasMaxLength(4000);

            entity.Property(e => e.Credentials).HasMaxLength(4000);
        });

        modelBuilder.Entity<SessionUser>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(100);

            entity.Property(e => e.Created).HasColumnType("timestamp with time zone");

            // NOTE(DG) - I'm unsure of the ValueComparer here
            entity
                .Property(e => e.Roles)
                .HasMaxLength(4000)
                .HasConversion(
                    modelValue => JsonConvert.SerializeObject(modelValue, JsonSettings),
                    dbValue => JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(dbValue, JsonSettings),
                    new ValueComparer<Dictionary<int, List<string>>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c));
        });

        modelBuilder.Entity<Space>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Customer })
                .HasName("Spaces_pkey");

            entity.Property(e => e.Created).HasColumnType("timestamp with time zone");

            entity.Property(e => e.ImageBucket)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Roles)
                .IsRequired()
                .HasMaxLength(1000)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    stringArrayComparer);

            entity.Property(e => e.Tags)
                .IsRequired()
                .HasMaxLength(1000)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    stringArrayComparer);

            entity.Ignore(e => e.ApproximateNumberOfImages);
        });

        modelBuilder.Entity<StoragePolicy>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);
        });

        modelBuilder.Entity<ThumbnailPolicy>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Sizes)
                .IsRequired()
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(500);

            entity.Property(e => e.Created).HasColumnType("timestamp with time zone");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.EncryptedPassword)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Roles)
                .IsRequired()
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<ImageOptimisationPolicy>().HasData(
            new ImageOptimisationPolicy
            {
                Id = "none", Customer = 1, Global = true, Name = "No optimisation/transcoding",
                TechnicalDetails = new[] { "no-op" }
            },
            new ImageOptimisationPolicy
            {
                Id = "use-original", Customer = 1, Global = true, Name = "Use original for image-server",
                TechnicalDetails = new[] { "use-original" }
            }
        );
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
