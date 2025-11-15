using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />

    public partial class InitialMigration : Migration
    {

        public partial class LastChanges : Migration

        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    name: "Account",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        Type = table.Column<int>(type: "int", nullable: false),
                        Status = table.Column<int>(type: "int", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                        AdminId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Account", x => x.Id);
                        table.UniqueConstraint("AK_Account_Number", x => x.Number);
                    });

                migrationBuilder.CreateTable(
                    name: "Beneficiarys",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        ClientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                        BeneficiaryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                        Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Beneficiarys", x => x.Id);
                    });

                migrationBuilder.CreateTable(
                    name: "Commerce",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        IsActive = table.Column<bool>(type: "bit", nullable: false),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Commerce", x => x.Id);
                    });

                migrationBuilder.CreateTable(
                    name: "CreditCard",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        CreditLimitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                        TotalAmountOwed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        CVC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Status = table.Column<int>(type: "int", nullable: false),
                        AdminId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_CreditCard", x => x.Id);
                        table.UniqueConstraint("AK_CreditCard_Number", x => x.Number);
                    });

                migrationBuilder.CreateTable(
                    name: "Loan",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                        ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        TotalLoanAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                        LoanTermInMonths = table.Column<int>(type: "int", nullable: false),
                        Status = table.Column<int>(type: "int", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                        IsActive = table.Column<bool>(type: "bit", nullable: false),
                        UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Loan", x => x.Id);
                    });

                migrationBuilder.CreateTable(
                    name: "Transaction",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                        Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                        Type = table.Column<int>(type: "int", nullable: false),
                        AccountNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        AccountId = table.Column<int>(type: "int", nullable: false),
                        Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Beneficiary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Status = table.Column<int>(type: "int", nullable: false),
                        Description = table.Column<int>(type: "int", maxLength: 100, nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Transaction", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Transaction_Account_AccountNumber",
                            column: x => x.AccountNumber,
                            principalTable: "Account",
                            principalColumn: "Number",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateTable(
                    name: "Purchase",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                        DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                        AmountSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        MerchantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        CardNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Purchase", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Purchase_CreditCard_CardNumber",
                            column: x => x.CardNumber,
                            principalTable: "CreditCard",
                            principalColumn: "Number",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateTable(
                    name: "Installment",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        PayDate = table.Column<DateOnly>(type: "date", nullable: false),
                        Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        IsPaid = table.Column<bool>(type: "bit", nullable: false),
                        IsDelinquent = table.Column<bool>(type: "bit", nullable: false),
                        LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                        Number = table.Column<int>(type: "int", nullable: false),
                        IsModified = table.Column<bool>(type: "bit", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Installment", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Installment_Loan_LoanId",
                            column: x => x.LoanId,
                            principalTable: "Loan",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateIndex(
                    name: "IX_Account_Number",
                    table: "Account",
                    column: "Number",
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_CreditCard_Number",
                    table: "CreditCard",
                    column: "Number",
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_Installment_LoanId",
                    table: "Installment",
                    column: "LoanId");

                migrationBuilder.CreateIndex(
                    name: "IX_Purchase_CardNumber",
                    table: "Purchase",
                    column: "CardNumber");

                migrationBuilder.CreateIndex(
                    name: "IX_Transaction_AccountNumber",
                    table: "Transaction",
                    column: "AccountNumber");
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropTable(
                    name: "Beneficiarys");

                migrationBuilder.DropTable(
                    name: "Commerce");

                migrationBuilder.DropTable(
                    name: "Installment");

                migrationBuilder.DropTable(
                    name: "Purchase");

                migrationBuilder.DropTable(
                    name: "Transaction");

                migrationBuilder.DropTable(
                    name: "Loan");

                migrationBuilder.DropTable(
                    name: "CreditCard");

                migrationBuilder.DropTable(
                    name: "Account");
            }
        }
    }
}