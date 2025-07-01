using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAISystemUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAtUtc", "DisplayName", "Email", "ExternalId", "IsActive", "LastSeenUtc", "ProviderName", "UpdatedAtUtc" },
                values: new object[] { new Guid("c82dcc58-9f57-4419-8439-94dff46dba5a"), null, new DateTime(2025, 7, 1, 6, 11, 9, 150, DateTimeKind.Utc).AddTicks(5703), "AI Assistant", "ai@chatapp.system", null, true, null, "System", new DateTime(2025, 7, 1, 6, 11, 9, 150, DateTimeKind.Utc).AddTicks(5703) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c82dcc58-9f57-4419-8439-94dff46dba5a"));
        }
    }
}
