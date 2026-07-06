using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable IDE0161 // Convert to file-scoped namespace
#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Infrastructure.Migrations
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
                name: "categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

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
                name: "tags",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    permission_version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_expenses_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_expenses_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalSchema: "public",
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_expenses_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_tags",
                schema: "public",
                columns: table => new
                {
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_tags", x => new { x.expense_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_expense_tags_expenses_expense_id",
                        column: x => x.expense_id,
                        principalSchema: "public",
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_expense_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "public",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "categories",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Alimentação" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Transporte" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Moradia" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Saúde" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Lazer" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Educação" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "payment_methods",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Crédito" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Débito" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Dinheiro" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Pix" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Boleto" }
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
                name: "ix_categories_name",
                schema: "public",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_tags_tag_id",
                schema: "public",
                table: "expense_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_category_id",
                schema: "public",
                table: "expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_payment_method_id",
                schema: "public",
                table: "expenses",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_user_id",
                schema: "public",
                table: "expenses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_name",
                schema: "public",
                table: "payment_methods",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                schema: "public",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                schema: "public",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "public",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_tags",
                schema: "public");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "expenses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payment_methods",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
