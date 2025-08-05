using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleManagementBackend.DAL.Migrations
{
    public partial class INITTIALMIGb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorWiseBoxs_BoxCategorys_CategoryId",
                table: "FloorWiseBoxs");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorWiseBoxs_Floors_FloorId",
                table: "FloorWiseBoxs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoxCategorys",
                table: "BoxCategorys");

            migrationBuilder.RenameTable(
                name: "BoxCategorys",
                newName: "BoxCategories");

            migrationBuilder.RenameColumn(
                name: "Lenght",
                table: "FloorWiseBoxs",
                newName: "Length");

            migrationBuilder.AlterColumn<int>(
                name: "FloorId",
                table: "FloorWiseBoxs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "FloorWiseBoxs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoxCategories",
                table: "BoxCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorWiseBoxs_BoxCategories_CategoryId",
                table: "FloorWiseBoxs",
                column: "CategoryId",
                principalTable: "BoxCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorWiseBoxs_Floors_FloorId",
                table: "FloorWiseBoxs",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorWiseBoxs_BoxCategories_CategoryId",
                table: "FloorWiseBoxs");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorWiseBoxs_Floors_FloorId",
                table: "FloorWiseBoxs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoxCategories",
                table: "BoxCategories");

            migrationBuilder.RenameTable(
                name: "BoxCategories",
                newName: "BoxCategorys");

            migrationBuilder.RenameColumn(
                name: "Length",
                table: "FloorWiseBoxs",
                newName: "Lenght");

            migrationBuilder.AlterColumn<int>(
                name: "FloorId",
                table: "FloorWiseBoxs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "FloorWiseBoxs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoxCategorys",
                table: "BoxCategorys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorWiseBoxs_BoxCategorys_CategoryId",
                table: "FloorWiseBoxs",
                column: "CategoryId",
                principalTable: "BoxCategorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorWiseBoxs_Floors_FloorId",
                table: "FloorWiseBoxs",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id");
        }
    }
}
