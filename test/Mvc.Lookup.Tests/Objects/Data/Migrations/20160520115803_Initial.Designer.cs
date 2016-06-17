using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NonFactors.Mvc.Lookup.Tests.Objects.Data.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20160520115803_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20901")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestModel", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("NullableString");

                    b.Property<int>("Number");

                    b.Property<string>("ParentId");

                    b.Property<string>("RelationId");

                    b.Property<decimal>("Sum");

                    b.HasKey("Id");

                    b.HasIndex("RelationId");

                    b.ToTable("TestModel");
                });

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestRelationModel", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("NoValue");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("TestRelationModel");
                });

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestModel", b =>
                {
                    b.HasOne("NonFactors.Mvc.Lookup.Tests.Objects.TestRelationModel")
                        .WithMany()
                        .HasForeignKey("RelationId");
                });
        }
    }
}
