#nullable disable

namespace Ai.Tutor.Infrastructure.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class OrgsRelations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_org_members_orgs_OrgId1",
            table: "org_members");

        migrationBuilder.DropIndex(
            name: "IX_org_members_OrgId1",
            table: "org_members");

        migrationBuilder.DropColumn(
            name: "OrgId1",
            table: "org_members");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
            .Annotation("Npgsql:Enum:folder_type_enum", "project,folder")
            .Annotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
            .Annotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
            .Annotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
            .Annotation("Npgsql:Enum:sender_type_enum", "user,ai")
            .Annotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted")
            .OldAnnotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
            .OldAnnotation("Npgsql:Enum:folder_type_enum", "project,folder")
            .OldAnnotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
            .OldAnnotation("Npgsql:Enum:org_type_enum", "personal,education,household,business");

        migrationBuilder.CreateTable(
            name: "chat_threads",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                org_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                sort_order = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1000m),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_chat_threads", x => x.id);
                table.ForeignKey(
                    name: "fk_threads_folder",
                    column: x => x.folder_id,
                    principalTable: "folders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_threads_org",
                    column: x => x.org_id,
                    principalTable: "orgs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_threads_user",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "chat_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                sender_type = table.Column<string>(type: "text", nullable: false),
                sender_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_chat_messages", x => x.id);
                table.ForeignKey(
                    name: "fk_messages_thread",
                    column: x => x.thread_id,
                    principalTable: "chat_threads",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_messages_thread_created_id",
            table: "chat_messages",
            columns: new[] { "thread_id", "created_at", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_chat_threads_folder_id",
            table: "chat_threads",
            column: "folder_id");

        migrationBuilder.CreateIndex(
            name: "ix_threads_org_folder_sort_id",
            table: "chat_threads",
            columns: new[] { "org_id", "folder_id", "sort_order", "id" });

        migrationBuilder.CreateIndex(
            name: "ix_threads_user_sort_id",
            table: "chat_threads",
            columns: new[] { "user_id", "sort_order", "id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "chat_messages");

        migrationBuilder.DropTable(
            name: "chat_threads");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
            .Annotation("Npgsql:Enum:folder_type_enum", "project,folder")
            .Annotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
            .Annotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
            .OldAnnotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
            .OldAnnotation("Npgsql:Enum:folder_type_enum", "project,folder")
            .OldAnnotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
            .OldAnnotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
            .OldAnnotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
            .OldAnnotation("Npgsql:Enum:sender_type_enum", "user,ai")
            .OldAnnotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted");

        migrationBuilder.AddColumn<Guid>(
            name: "OrgId1",
            table: "org_members",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_org_members_OrgId1",
            table: "org_members",
            column: "OrgId1");

        migrationBuilder.AddForeignKey(
            name: "FK_org_members_orgs_OrgId1",
            table: "org_members",
            column: "OrgId1",
            principalTable: "orgs",
            principalColumn: "id");
    }
}