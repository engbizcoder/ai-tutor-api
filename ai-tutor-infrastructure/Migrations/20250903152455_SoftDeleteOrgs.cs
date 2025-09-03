using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ai.Tutor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteOrgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
                .Annotation("Npgsql:Enum:folder_type_enum", "project,folder")
                .Annotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
                .Annotation("Npgsql:Enum:org_lifecycle_status_enum", "active,disabled,deleted,purged")
                .Annotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
                .Annotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
                .Annotation("Npgsql:Enum:sender_type_enum", "user,ai")
                .Annotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted")
                .OldAnnotation("Npgsql:Enum:folder_status_enum", "active,archived,deleted")
                .OldAnnotation("Npgsql:Enum:folder_type_enum", "project,folder")
                .OldAnnotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
                .OldAnnotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
                .OldAnnotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
                .OldAnnotation("Npgsql:Enum:sender_type_enum", "user,ai")
                .OldAnnotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:message_status_enum", "sending,sent,error")
                .OldAnnotation("Npgsql:Enum:org_lifecycle_status_enum", "active,disabled,deleted,purged")
                .OldAnnotation("Npgsql:Enum:org_role_enum", "owner,admin,member")
                .OldAnnotation("Npgsql:Enum:org_type_enum", "personal,education,household,business")
                .OldAnnotation("Npgsql:Enum:sender_type_enum", "user,ai")
                .OldAnnotation("Npgsql:Enum:thread_status_enum", "active,archived,deleted");
        }
    }
}
