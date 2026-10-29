using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class AlignTP4OrdinateurModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Ordinateurs SET Etat = 3 WHERE Etat IN (0, 1, 2);");

            migrationBuilder.DropColumn(
                name: "Marque",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "Processeur",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "RAM",
                table: "Ordinateurs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marque",
                table: "Ordinateurs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Processeur",
                table: "Ordinateurs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RAM",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
