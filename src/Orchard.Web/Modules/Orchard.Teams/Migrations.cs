using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Teams.Migration {
    public class UsersDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("TeamPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("TeamName")
                    .Column<string>("NormalizedTeamName")
                    .Column<string>("Email")
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("LastLoginUtc")
                    .Column<DateTime>("LastLogoutUtc")
                );
            SchemaBuilder.CreateTable("TeamMembersPartRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int> ("TeamMemberType", c => c.NotNull().WithDefault(0)) // 0=User, 1 = Team
                    .Column<int> ("TeamId", c => c.NotNull())
                    .Column<int>("UserId", c => c.NotNull())
                );

            ContentDefinitionManager.AlterTypeDefinition("Team", cfg => cfg.Creatable(false));
            return 5;
        }
        public int UpdateFrom1() {
            SchemaBuilder.DropTable("UserTeamsPartRecord");
            SchemaBuilder.CreateTable("TeamUsersPartRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TeamId")
                    .Column<int>("UserId")
                );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.DropTable("TeamUsersPartRecord");
            SchemaBuilder.CreateTable("TeamUsersPartRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TeamId", c => c.NotNull())
                    .Column<int>("UserId", c => c.NotNull())
                );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable ("TeamUsersPartRecord",
                table => table
                    .AddColumn<int> ("TeamMemberType", c => c.NotNull ().WithDefault (0))); // User
            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.DropTable ("TeamUsersPartRecord");
            SchemaBuilder.CreateTable ("TeamMembersPartRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int> ("TeamMemberType", c => c.NotNull().WithDefault(0)) // 0=User, 1 = Team
                    .Column<int> ("TeamId", c => c.NotNull())
                    .Column<int>("UserId", c => c.NotNull())
                );

            return 5;
        }
}
}