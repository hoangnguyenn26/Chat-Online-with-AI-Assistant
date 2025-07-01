using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAISystemUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c82dcc58-9f57-4419-8439-94dff46dba5a"),
                columns: new[] { "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c82dcc58-9f57-4419-8439-94dff46dba5a"),
                columns: new[] { "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[] { new DateTime(2025, 7, 1, 6, 11, 9, 150, DateTimeKind.Utc).AddTicks(5703), new DateTime(2025, 7, 1, 6, 11, 9, 150, DateTimeKind.Utc).AddTicks(5703) });
        }
    }
}
