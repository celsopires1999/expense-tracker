using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace Infrastructure.Database.Migrations;

public partial class Create_Database : Migration
{
    private static void InsertPermission(MigrationBuilder migrationBuilder, string roleId, string permission)
    {
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "role_permissions",
            columns: ["role_id", "permission"],
            values: new object[] { new Guid(roleId), permission });
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.EnsureSchema(name: "public");

        _ = migrationBuilder.CreateTable(
            name: "categories",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_categories", x => x.id));

        _ = migrationBuilder.CreateTable(
            name: "payment_methods",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_payment_methods", x => x.id));

        _ = migrationBuilder.CreateTable(
            name: "roles",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_roles", x => x.id));

        _ = migrationBuilder.CreateTable(
            name: "tags",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_tags", x => x.id));

        _ = migrationBuilder.CreateTable(
            name: "users",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                first_name = table.Column<string>(type: "text", nullable: false),
                last_name = table.Column<string>(type: "text", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                permission_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
            },
            constraints: table => table.PrimaryKey("pk_users", x => x.id));

        _ = migrationBuilder.CreateTable(
            name: "role_permissions",
            schema: "public",
            columns: table => new
            {
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission });
                _ = table.ForeignKey(
                    name: "fk_role_permissions_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "public",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "user_roles",
            schema: "public",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                _ = table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "fk_user_roles_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "public",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
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
                _ = table.PrimaryKey("pk_expenses", x => x.id);
                _ = table.ForeignKey(
                    name: "fk_expenses_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "fk_expenses_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "public",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "fk_expenses_payment_methods_payment_method_id",
                    column: x => x.payment_method_id,
                    principalSchema: "public",
                    principalTable: "payment_methods",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Seed: Admin role
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "roles",
            columns: ["id", "name"],
            values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" });

        // Seed: Viewer role
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "roles",
            columns: ["id", "name"],
            values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Viewer" });

        // Seed: Admin permissions (all)
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:create");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:read");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:read:all");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:update");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:update:all");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:delete");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "expenses:delete:all");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "categories:create");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "categories:read");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "categories:update");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "categories:delete");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "payment-methods:create");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "payment-methods:read");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "payment-methods:update");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "payment-methods:delete");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "tags:create");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "tags:read");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "tags:update");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "tags:delete");
        InsertPermission(migrationBuilder, "11111111-1111-1111-1111-111111111111", "users:access");

        // Seed: Viewer permissions (read-only)
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "expenses:read");
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "expenses:read:all");
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "categories:read");
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "payment-methods:read");
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "tags:read");
        InsertPermission(migrationBuilder, "22222222-2222-2222-2222-222222222222", "users:access");

        // Seed: Default categories
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "Alimentação" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), "Transporte" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), "Moradia" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("66666666-6666-6666-6666-666666666666"), "Saúde" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("77777777-7777-7777-7777-777777777777"), "Lazer" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "categories",
            columns: ["id", "name"],
            values: new object[] { new Guid("88888888-8888-8888-8888-888888888888"), "Educação" });

        // Seed: Default payment methods
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "payment_methods",
            columns: ["id", "name"],
            values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Crédito" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "payment_methods",
            columns: ["id", "name"],
            values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Débito" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "payment_methods",
            columns: ["id", "name"],
            values: new object[] { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Dinheiro" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "payment_methods",
            columns: ["id", "name"],
            values: new object[] { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Pix" });
        _ = migrationBuilder.InsertData(
            schema: "public",
            table: "payment_methods",
            columns: ["id", "name"],
            values: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Boleto" });

        _ = migrationBuilder.CreateTable(
            name: "expense_tag",
            schema: "public",
            columns: table => new
            {
                expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                tag_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("pk_expense_tag", x => new { x.expense_id, x.tag_id });
                _ = table.ForeignKey(
                    name: "fk_expense_tag_expenses_expense_id",
                    column: x => x.expense_id,
                    principalSchema: "public",
                    principalTable: "expenses",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "fk_expense_tag_tags_tag_id",
                    column: x => x.tag_id,
                    principalSchema: "public",
                    principalTable: "tags",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(name: "expense_tag", schema: "public");
        _ = migrationBuilder.DropTable(name: "expenses", schema: "public");
        _ = migrationBuilder.DropTable(name: "role_permissions", schema: "public");
        _ = migrationBuilder.DropTable(name: "user_roles", schema: "public");
        _ = migrationBuilder.DropTable(name: "categories", schema: "public");
        _ = migrationBuilder.DropTable(name: "payment_methods", schema: "public");
        _ = migrationBuilder.DropTable(name: "tags", schema: "public");
        _ = migrationBuilder.DropTable(name: "users", schema: "public");
        _ = migrationBuilder.DropTable(name: "roles", schema: "public");
    }
}
