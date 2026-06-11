using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPesapalTransactionMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalTrackingId",
                table: "Transactions",
                newName: "OrderTrackingId");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "Transactions",
                newName: "MerchantReference");

            migrationBuilder.RenameColumn(
                name: "ProviderReference",
                table: "Transactions",
                newName: "TransactionReference");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Reference",
                table: "Transactions",
                newName: "IX_Transactions_MerchantReference");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Transactions",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KNOWLEDGE_BASE",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KNOWLEDGE_BASE", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KNOWLEDGE_BASE");

            migrationBuilder.RenameColumn(
                name: "OrderTrackingId",
                table: "Transactions",
                newName: "ExternalTrackingId");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "TransactionReference",
                table: "Transactions",
                newName: "ProviderReference");

            migrationBuilder.RenameColumn(
                name: "MerchantReference",
                table: "Transactions",
                newName: "Reference");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_MerchantReference",
                table: "Transactions",
                newName: "IX_Transactions_Reference");
        }
    }
}
