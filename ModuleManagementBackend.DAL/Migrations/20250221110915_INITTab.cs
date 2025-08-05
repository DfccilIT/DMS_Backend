using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleManagementBackend.DAL.Migrations
{
    public partial class INITTab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assets_cabins_CabinId",
                table: "assets");

            migrationBuilder.DropForeignKey(
                name: "FK_cabins_floors_FloorId",
                table: "cabins");

            migrationBuilder.DropForeignKey(
                name: "FK_doors_walls_WallId",
                table: "doors");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_cabins_CabinId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_floors_buildings_BuildingId",
                table: "floors");

            migrationBuilder.DropForeignKey(
                name: "FK_pillars_floors_FloorId",
                table: "pillars");

            migrationBuilder.DropForeignKey(
                name: "FK_walls_floors_FloorId",
                table: "walls");

            migrationBuilder.DropForeignKey(
                name: "FK_windows_walls_WallId",
                table: "windows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_windows",
                table: "windows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_walls",
                table: "walls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pillars",
                table: "pillars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_floors",
                table: "floors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_doors",
                table: "doors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cabins",
                table: "cabins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_buildings",
                table: "buildings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_assets",
                table: "assets");

            migrationBuilder.RenameTable(
                name: "windows",
                newName: "Windows");

            migrationBuilder.RenameTable(
                name: "walls",
                newName: "Walls");

            migrationBuilder.RenameTable(
                name: "pillars",
                newName: "Pillars");

            migrationBuilder.RenameTable(
                name: "floors",
                newName: "Floors");

            migrationBuilder.RenameTable(
                name: "employees",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "doors",
                newName: "Doors");

            migrationBuilder.RenameTable(
                name: "cabins",
                newName: "Cabins");

            migrationBuilder.RenameTable(
                name: "buildings",
                newName: "Buildings");

            migrationBuilder.RenameTable(
                name: "assets",
                newName: "Assets");

            migrationBuilder.RenameIndex(
                name: "IX_windows_WallId",
                table: "Windows",
                newName: "IX_Windows_WallId");

            migrationBuilder.RenameIndex(
                name: "IX_walls_FloorId",
                table: "Walls",
                newName: "IX_Walls_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_pillars_FloorId",
                table: "Pillars",
                newName: "IX_Pillars_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_floors_BuildingId",
                table: "Floors",
                newName: "IX_Floors_BuildingId");

            migrationBuilder.RenameIndex(
                name: "IX_employees_CabinId",
                table: "Employees",
                newName: "IX_Employees_CabinId");

            migrationBuilder.RenameIndex(
                name: "IX_doors_WallId",
                table: "Doors",
                newName: "IX_Doors_WallId");

            migrationBuilder.RenameIndex(
                name: "IX_cabins_FloorId",
                table: "Cabins",
                newName: "IX_Cabins_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_assets_CabinId",
                table: "Assets",
                newName: "IX_Assets_CabinId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Windows",
                table: "Windows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Walls",
                table: "Walls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pillars",
                table: "Pillars",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Floors",
                table: "Floors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doors",
                table: "Doors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cabins",
                table: "Cabins",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Buildings",
                table: "Buildings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assets",
                table: "Assets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Cabins_CabinId",
                table: "Assets",
                column: "CabinId",
                principalTable: "Cabins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cabins_Floors_FloorId",
                table: "Cabins",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doors_Walls_WallId",
                table: "Doors",
                column: "WallId",
                principalTable: "Walls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Cabins_CabinId",
                table: "Employees",
                column: "CabinId",
                principalTable: "Cabins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Floors_Buildings_BuildingId",
                table: "Floors",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pillars_Floors_FloorId",
                table: "Pillars",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Walls_Floors_FloorId",
                table: "Walls",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Windows_Walls_WallId",
                table: "Windows",
                column: "WallId",
                principalTable: "Walls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Cabins_CabinId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Cabins_Floors_FloorId",
                table: "Cabins");

            migrationBuilder.DropForeignKey(
                name: "FK_Doors_Walls_WallId",
                table: "Doors");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Cabins_CabinId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Floors_Buildings_BuildingId",
                table: "Floors");

            migrationBuilder.DropForeignKey(
                name: "FK_Pillars_Floors_FloorId",
                table: "Pillars");

            migrationBuilder.DropForeignKey(
                name: "FK_Walls_Floors_FloorId",
                table: "Walls");

            migrationBuilder.DropForeignKey(
                name: "FK_Windows_Walls_WallId",
                table: "Windows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Windows",
                table: "Windows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Walls",
                table: "Walls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pillars",
                table: "Pillars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Floors",
                table: "Floors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doors",
                table: "Doors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cabins",
                table: "Cabins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Buildings",
                table: "Buildings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assets",
                table: "Assets");

            migrationBuilder.RenameTable(
                name: "Windows",
                newName: "windows");

            migrationBuilder.RenameTable(
                name: "Walls",
                newName: "walls");

            migrationBuilder.RenameTable(
                name: "Pillars",
                newName: "pillars");

            migrationBuilder.RenameTable(
                name: "Floors",
                newName: "floors");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "employees");

            migrationBuilder.RenameTable(
                name: "Doors",
                newName: "doors");

            migrationBuilder.RenameTable(
                name: "Cabins",
                newName: "cabins");

            migrationBuilder.RenameTable(
                name: "Buildings",
                newName: "buildings");

            migrationBuilder.RenameTable(
                name: "Assets",
                newName: "assets");

            migrationBuilder.RenameIndex(
                name: "IX_Windows_WallId",
                table: "windows",
                newName: "IX_windows_WallId");

            migrationBuilder.RenameIndex(
                name: "IX_Walls_FloorId",
                table: "walls",
                newName: "IX_walls_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_Pillars_FloorId",
                table: "pillars",
                newName: "IX_pillars_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_Floors_BuildingId",
                table: "floors",
                newName: "IX_floors_BuildingId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_CabinId",
                table: "employees",
                newName: "IX_employees_CabinId");

            migrationBuilder.RenameIndex(
                name: "IX_Doors_WallId",
                table: "doors",
                newName: "IX_doors_WallId");

            migrationBuilder.RenameIndex(
                name: "IX_Cabins_FloorId",
                table: "cabins",
                newName: "IX_cabins_FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_Assets_CabinId",
                table: "assets",
                newName: "IX_assets_CabinId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_windows",
                table: "windows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_walls",
                table: "walls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pillars",
                table: "pillars",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_floors",
                table: "floors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                table: "employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_doors",
                table: "doors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cabins",
                table: "cabins",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_buildings",
                table: "buildings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_assets",
                table: "assets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_assets_cabins_CabinId",
                table: "assets",
                column: "CabinId",
                principalTable: "cabins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cabins_floors_FloorId",
                table: "cabins",
                column: "FloorId",
                principalTable: "floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_doors_walls_WallId",
                table: "doors",
                column: "WallId",
                principalTable: "walls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_cabins_CabinId",
                table: "employees",
                column: "CabinId",
                principalTable: "cabins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_floors_buildings_BuildingId",
                table: "floors",
                column: "BuildingId",
                principalTable: "buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pillars_floors_FloorId",
                table: "pillars",
                column: "FloorId",
                principalTable: "floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_walls_floors_FloorId",
                table: "walls",
                column: "FloorId",
                principalTable: "floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_windows_walls_WallId",
                table: "windows",
                column: "WallId",
                principalTable: "walls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
