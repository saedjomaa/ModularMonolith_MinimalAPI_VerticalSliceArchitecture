using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseHub.Courses.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261128014750_InitialCourses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "courses");

        migrationBuilder.CreateTable(
            name: "Courses",
            schema: "courses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                Capacity = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Courses_InstructorId",
            schema: "courses",
            table: "Courses",
            column: "InstructorId");

        migrationBuilder.CreateIndex(
            name: "IX_Courses_Status",
            schema: "courses",
            table: "Courses",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Courses",
            schema: "courses");
    }
}
