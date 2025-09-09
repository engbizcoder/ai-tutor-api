using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ai.Tutor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attachment_type_enum", "document,image,other")
                .Annotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
                .Annotation("Npgsql:Enum:folder_type_enum", "project,folder")
                .Annotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
                .Annotation("Npgsql:Enum:org_lifecycle_status_enum", "active,disabled,deleted,purged")
                .Annotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
                .Annotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
                .Annotation("Npgsql:Enum:reference_type_enum", "file,page,video,link,formula")
                .Annotation("Npgsql:Enum:sender_type_enum", "user,ai")
                .Annotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted");

            migrationBuilder.CreateTable(
                name: "orgs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    LifecycleStatus = table.Column<int>(type: "integer", nullable: false),
                    DisabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PurgeScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetentionDays = table.Column<int>(type: "integer", nullable: false),
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
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    storage_url = table.Column<string>(type: "text", nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    checksum_sha256 = table.Column<string>(type: "text", nullable: true),
                    pages = table.Column<int>(type: "integer", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrgId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_files_orgs_OrgId1",
                        column: x => x.OrgId1,
                        principalTable: "orgs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_files_users_OwnerUserId1",
                        column: x => x.OwnerUserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_files_org",
                        column: x => x.org_id,
                        principalTable: "orgs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_files_owner",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_members", x => new { x.org_id, x.user_id });
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
                    IdempotencyKey = table.Column<string>(type: "text", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MessageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    FileId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_attachments_chat_messages_MessageId1",
                        column: x => x.MessageId1,
                        principalTable: "chat_messages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_attachments_files_FileId1",
                        column: x => x.FileId1,
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_attachments_file",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attachments_message",
                        column: x => x.message_id,
                        principalTable: "chat_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: true),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    page_number = table.Column<int>(type: "integer", nullable: true),
                    preview_img_url = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ThreadId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    FileId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_references", x => x.id);
                    table.CheckConstraint("ck_document_references_url_or_file", "(url IS NOT NULL) OR (file_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_document_references_chat_messages_MessageId1",
                        column: x => x.MessageId1,
                        principalTable: "chat_messages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_document_references_chat_threads_ThreadId1",
                        column: x => x.ThreadId1,
                        principalTable: "chat_threads",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_document_references_files_FileId1",
                        column: x => x.FileId1,
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_document_references_file",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_references_message",
                        column: x => x.message_id,
                        principalTable: "chat_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_references_thread",
                        column: x => x.thread_id,
                        principalTable: "chat_threads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_file_id",
                table: "attachments",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_FileId1",
                table: "attachments",
                column: "FileId1");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_message",
                table: "attachments",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_MessageId1",
                table: "attachments",
                column: "MessageId1");

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

            migrationBuilder.CreateIndex(
                name: "ix_document_references_file_id",
                table: "document_references",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_references_FileId1",
                table: "document_references",
                column: "FileId1");

            migrationBuilder.CreateIndex(
                name: "ix_document_references_message",
                table: "document_references",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_references_MessageId1",
                table: "document_references",
                column: "MessageId1");

            migrationBuilder.CreateIndex(
                name: "ix_document_references_thread_created_id",
                table: "document_references",
                columns: new[] { "thread_id", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_document_references_ThreadId1",
                table: "document_references",
                column: "ThreadId1");

            migrationBuilder.CreateIndex(
                name: "ix_files_checksum",
                table: "files",
                column: "checksum_sha256");

            migrationBuilder.CreateIndex(
                name: "ix_files_org_owner",
                table: "files",
                columns: new[] { "org_id", "owner_user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_files_OrgId1",
                table: "files",
                column: "OrgId1");

            migrationBuilder.CreateIndex(
                name: "IX_files_owner_user_id",
                table: "files",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_OwnerUserId1",
                table: "files",
                column: "OwnerUserId1");

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
                name: "attachments");

            migrationBuilder.DropTable(
                name: "document_references");

            migrationBuilder.DropTable(
                name: "org_members");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "chat_threads");

            migrationBuilder.DropTable(
                name: "folders");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "orgs");
        }
    }
}
