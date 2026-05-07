using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace detox.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultipleDurationUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationInHours",
                table: "FocusBlocks",
                newName: "DurationValue");

            migrationBuilder.AddColumn<string>(
                name: "DurationUnit",
                table: "FocusBlocks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationUnit",
                table: "FocusBlocks");

            migrationBuilder.RenameColumn(
                name: "DurationValue",
                table: "FocusBlocks",
                newName: "DurationInHours");
        }
    }
}
