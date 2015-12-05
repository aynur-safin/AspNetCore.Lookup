using Microsoft.Data.Entity.Migrations;
using System;

namespace Mvc.Lookup.Tests.Objects.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRelationModel",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    NoValue = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRelationModel", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "TestModel",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    FirstRelationModelId = table.Column<string>(nullable: true),
                    NullableString = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    ParentId = table.Column<string>(nullable: true),
                    SecondRelationModelId = table.Column<string>(nullable: true),
                    Sum = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestModel_TestRelationModel_FirstRelationModelId",
                        column: x => x.FirstRelationModelId,
                        principalTable: "TestRelationModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestModel_TestRelationModel_SecondRelationModelId",
                        column: x => x.SecondRelationModelId,
                        principalTable: "TestRelationModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("TestModel");
            migrationBuilder.DropTable("TestRelationModel");
        }
    }
}
