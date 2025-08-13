using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ai.Tutor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
                .Annotation("Npgsql:Enum:folder_type_enum", "project,folder")
                .Annotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
                .Annotation("Npgsql:Enum:org_type_enum", "personal,education,household,business");

            migrationBuilder.CreateTable(
                name: "orgs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orgs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    primary_org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_primary_org",
                        column: x => x.primary_org_id,
                        principalTable: "orgs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "folders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, defaultValue: 1000m),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_folders", x => x.id);
                    table.ForeignKey(
                        name: "fk_folders_orgs",
                        column: x => x.org_id,
                        principalTable: "orgs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_folders_parent",
                        column: x => x.parent_id,
                        principalTable: "folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_folders_users_owner",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "org_members",
                columns: table => new
                {
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrgId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_members", x => new { x.org_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_org_members_orgs_OrgId1",
                        column: x => x.OrgId1,
                        principalTable: "orgs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_org_members_orgs",
                        column: x => x.org_id,
                        principalTable: "orgs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_members_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_folders_org_owner_parent",
                table: "folders",
                columns: new[] { "org_id", "owner_user_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_folders_parent_id",
                table: "folders",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ux_folders_owner_parent_name",
                table: "folders",
                columns: new[] { "owner_user_id", "parent_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_org_members_OrgId1",
                table: "org_members",
                column: "OrgId1");

            migrationBuilder.CreateIndex(
                name: "IX_org_members_user_id",
                table: "org_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_orgs_slug",
                table: "orgs",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_primary_org_id",
                table: "users",
                column: "primary_org_id");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "folders");

            migrationBuilder.DropTable(
                name: "org_members");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "orgs");
        }
    }
}
