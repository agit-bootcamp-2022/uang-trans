using Microsoft.EntityFrameworkCore.Migrations;

namespace uang_trans.Migrations
{
    public partial class UpdateWalletMutation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Debit",
                table: "WalletMutations",
                newName: "Amount");

            migrationBuilder.AlterColumn<double>(
                name: "Balance",
                table: "Wallets",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MutationType",
                table: "WalletMutations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MutationType",
                table: "WalletMutations");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "WalletMutations",
                newName: "Debit");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "Wallets",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
