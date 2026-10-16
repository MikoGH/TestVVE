using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestVVE.Migrations
{
    /// <inheritdoc />
    public partial class m002_AddedVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "task_items",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "task_items");
        }
    }
}
