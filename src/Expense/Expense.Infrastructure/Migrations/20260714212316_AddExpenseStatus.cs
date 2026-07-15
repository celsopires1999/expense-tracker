using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "public",
                table: "expenses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql("UPDATE expenses SET status = 'Paid';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "expenses");
        }
    }
}
