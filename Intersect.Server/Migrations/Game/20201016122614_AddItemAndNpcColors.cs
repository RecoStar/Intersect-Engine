﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Intersect.Server.Migrations.Game
{
    public partial class AddItemAndNpcColors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Npcs",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Items",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Npcs");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Items");
        }
    }
}
