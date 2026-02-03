using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIdToCanvasManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Canvases",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_TaskId",
                table: "Canvases",
                column: "TaskId",
                unique: true,
                filter: "\"TaskId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Canvases_Tasks_TaskId",
                table: "Canvases",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Canvases_Tasks_TaskId",
                table: "Canvases");

            migrationBuilder.DropIndex(
                name: "IX_Canvases_TaskId",
                table: "Canvases");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Canvases");
        }
    }
}
