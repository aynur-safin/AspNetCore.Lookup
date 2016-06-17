using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NonFactors.Mvc.Lookup.Tests.Objects.Data.Migrations
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
                    NullableString = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    ParentId = table.Column<string>(nullable: true),
                    RelationId = table.Column<string>(nullable: true),
                    Sum = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestModel_TestRelationModel_RelationId",
                        column: x => x.RelationId,
                        principalTable: "TestRelationModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestModel_RelationId",
                table: "TestModel",
                column: "RelationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestModel");

            migrationBuilder.DropTable(
                name: "TestRelationModel");
        }
    }
}
