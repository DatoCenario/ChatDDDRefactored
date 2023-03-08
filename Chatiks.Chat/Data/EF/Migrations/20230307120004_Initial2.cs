using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chatiks.Chat.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbstractClass",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Field = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbstractClass", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Class1",
                columns: table => new
                {
                    Class1Id = table.Column<int>(type: "integer", nullable: false),
                    Field1 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class1", x => x.Class1Id);
                    table.ForeignKey(
                        name: "FK_Class1_AbstractClass_Class1Id",
                        column: x => x.Class1Id,
                        principalTable: "AbstractClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Class2",
                columns: table => new
                {
                    Class2Id = table.Column<int>(type: "integer", nullable: false),
                    Field2 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class2", x => x.Class2Id);
                    table.ForeignKey(
                        name: "FK_Class2_AbstractClass_Class2Id",
                        column: x => x.Class2Id,
                        principalTable: "AbstractClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Class1");

            migrationBuilder.DropTable(
                name: "Class2");

            migrationBuilder.DropTable(
                name: "AbstractClass");
        }
    }
}
