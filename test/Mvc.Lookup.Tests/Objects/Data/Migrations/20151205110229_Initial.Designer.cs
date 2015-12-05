using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NonFactors.Mvc.Lookup.Tests.Objects.Data;

namespace Mvc.Lookup.Tests.Objects.Data.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20151205110229_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestModel", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("FirstRelationModelId");

                    b.Property<string>("NullableString");

                    b.Property<int>("Number");

                    b.Property<string>("ParentId");

                    b.Property<string>("SecondRelationModelId");

                    b.Property<decimal>("Sum");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestRelationModel", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("NoValue");

                    b.Property<string>("Value");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("NonFactors.Mvc.Lookup.Tests.Objects.TestModel", b =>
                {
                    b.HasOne("NonFactors.Mvc.Lookup.Tests.Objects.TestRelationModel")
                        .WithMany()
                        .HasForeignKey("FirstRelationModelId");

                    b.HasOne("NonFactors.Mvc.Lookup.Tests.Objects.TestRelationModel")
                        .WithMany()
                        .HasForeignKey("SecondRelationModelId");
                });
        }
    }
}
