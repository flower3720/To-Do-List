using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "课程学习", 1 },
                    { 2, new DateTime(2026, 1, 1, 8, 5, 0, 0, DateTimeKind.Unspecified), "校园生活", 2 },
                    { 3, new DateTime(2026, 1, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "实习求职", 3 }
                });

            migrationBuilder.InsertData(
                table: "TodoItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "DueDate", "IsCompleted", "Priority", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "包含需求分析、系统设计、接口说明与测试记录。", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, "完成《Web 应用开发》课程设计报告", null },
                    { 2, 1, new DateTime(2026, 1, 2, 10, 30, 0, 0, DateTimeKind.Unspecified), "重点：二叉树遍历、图的 Dijkstra 算法。", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, "复习数据结构期末考点", null },
                    { 3, 1, new DateTime(2026, 1, 3, 14, 0, 0, 0, DateTimeKind.Unspecified), "打包上传到课程平台。", null, true, 1, "提交实验三源代码", new DateTime(2026, 1, 5, 20, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 2, new DateTime(2026, 1, 4, 8, 15, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, "预约图书馆研讨室", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CategoryId_IsCompleted",
                table: "TodoItems",
                columns: new[] { "CategoryId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_IsCompleted",
                table: "TodoItems",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
