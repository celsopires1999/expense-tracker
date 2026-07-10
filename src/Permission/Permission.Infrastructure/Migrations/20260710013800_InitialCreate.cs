using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Permission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "public",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission });
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Viewer" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Standard" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "role_permissions",
                columns: new[] { "permission", "role_id" },
                values: new object[,]
                {
                    { "categories:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "categories:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "categories:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "categories:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:delete:all", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:read:all", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "expenses:update:all", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "payment-methods:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "payment-methods:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "payment-methods:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "payment-methods:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "tags:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "tags:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "tags:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "tags:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "users:access", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "categories:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "expenses:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "expenses:read:all", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "payment-methods:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "tags:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "users:access", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "categories:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "expenses:create", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "expenses:delete", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "expenses:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "expenses:update", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "payment-methods:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "tags:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "users:access", new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                schema: "public",
                table: "roles",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");
        }
    }
}
