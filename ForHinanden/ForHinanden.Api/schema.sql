CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "HelpRequests" (
    "Id" uuid NOT NULL,
    "Title" text NOT NULL,
    "Description" text NOT NULL,
    "RequestedBy" text NOT NULL,
    "AcceptedBy" text,
    "IsAccepted" boolean NOT NULL,
    CONSTRAINT "PK_HelpRequests" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250903143327_InitialCreate', '9.0.8');

CREATE TABLE "UserProfiles" (
    "Id" uuid NOT NULL,
    "UserName" text NOT NULL,
    "FullName" text,
    "ProfilePictureUrl" text,
    "Bio" text,
    "TasksCreated" integer NOT NULL,
    "TasksCompleted" integer NOT NULL,
    "Rating" double precision,
    CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250917102317_AddUserProfile', '9.0.8');

CREATE TABLE "Messages" (
    "Id" uuid NOT NULL,
    "TaskId" uuid NOT NULL,
    "Sender" text NOT NULL,
    "Receiver" text NOT NULL,
    "Content" text NOT NULL,
    "SentAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Messages_HelpRequests_TaskId" FOREIGN KEY ("TaskId") REFERENCES "HelpRequests" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Messages_TaskId" ON "Messages" ("TaskId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250917110215_AddMessages', '9.0.8');

CREATE TABLE "Ratings" (
    "Id" uuid NOT NULL,
    "UserProfileId" uuid NOT NULL,
    "RatedBy" text NOT NULL,
    "Stars" integer NOT NULL,
    "Comment" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Ratings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Ratings_UserProfiles_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES "UserProfiles" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Ratings_UserProfileId" ON "Ratings" ("UserProfileId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250917121347_AddRatings', '9.0.8');

ALTER TABLE "Ratings" DROP CONSTRAINT "FK_Ratings_UserProfiles_UserProfileId";

DROP INDEX "IX_Ratings_UserProfileId";

ALTER TABLE "Ratings" RENAME COLUMN "UserProfileId" TO "TaskId";

ALTER TABLE "Ratings" ADD "ToUserId" text NOT NULL DEFAULT '';

ALTER TABLE "HelpRequests" ADD "Categories" text[] NOT NULL;

ALTER TABLE "HelpRequests" ADD "City" text NOT NULL DEFAULT '';

ALTER TABLE "HelpRequests" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "HelpRequests" ADD "Priority" integer NOT NULL DEFAULT 0;

CREATE TABLE "TaskOffers" (
    "Id" uuid NOT NULL,
    "TaskId" uuid NOT NULL,
    "OfferedBy" text NOT NULL,
    "Message" text,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_TaskOffers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TaskOffers_HelpRequests_TaskId" FOREIGN KEY ("TaskId") REFERENCES "HelpRequests" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_TaskOffers_TaskId_OfferedBy" ON "TaskOffers" ("TaskId", "OfferedBy");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250924094625_Fix_Offers_ReturnAndOwnerQuery', '9.0.8');

ALTER TABLE "HelpRequests" ADD "Duration" integer NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250926112132_Add_TaskDuration_To_HelpRequests', '9.0.8');

DROP TABLE "UserProfiles";

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "City" text NOT NULL,
    "ProfilePictureUrl" text,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250926132613_ReplaceUserProfilesWithUsers', '9.0.8');

ALTER TABLE "Users" DROP CONSTRAINT "PK_Users";

ALTER TABLE "Users" DROP COLUMN "Id";

ALTER TABLE "Users" ADD "DeviceId" text NOT NULL DEFAULT '';

ALTER TABLE "Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY ("DeviceId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250926133541_UsersWithDeviceIdKey', '9.0.8');

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

CREATE TABLE "Cities" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    CONSTRAINT "PK_Cities" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name");

CREATE UNIQUE INDEX "IX_Cities_Name" ON "Cities" ("Name");

ALTER TABLE "HelpRequests" ADD "CityId" uuid;

CREATE INDEX "IX_HelpRequests_CityId" ON "HelpRequests" ("CityId");

ALTER TABLE "HelpRequests" ADD CONSTRAINT "FK_HelpRequests_Cities_CityId" FOREIGN KEY ("CityId") REFERENCES "Cities" ("Id") ON DELETE RESTRICT;

CREATE TABLE "TaskCategories" (
    "Id" uuid NOT NULL,
    "TaskId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    CONSTRAINT "PK_TaskCategories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TaskCategories_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TaskCategories_HelpRequests_TaskId" FOREIGN KEY ("TaskId") REFERENCES "HelpRequests" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_TaskCategories_CategoryId" ON "TaskCategories" ("CategoryId");

CREATE UNIQUE INDEX "IX_TaskCategories_TaskId_CategoryId" ON "TaskCategories" ("TaskId", "CategoryId");

ALTER TABLE "HelpRequests" DROP COLUMN "Categories";

ALTER TABLE "HelpRequests" DROP COLUMN "City";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250927162914_Normalize_Tasks_AddCityAndCategories', '9.0.8');

CREATE TABLE "DurationOptions" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_DurationOptions" PRIMARY KEY ("Id")
);

CREATE TABLE "PriorityOptions" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_PriorityOptions" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_DurationOptions_Name" ON "DurationOptions" ("Name");

CREATE UNIQUE INDEX "IX_PriorityOptions_Name" ON "PriorityOptions" ("Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250927165406_Add_OptionTables_And_Indexes', '9.0.8');

ALTER TABLE "HelpRequests" DROP CONSTRAINT "FK_HelpRequests_Cities_CityId";

ALTER TABLE "Messages" DROP CONSTRAINT "FK_Messages_HelpRequests_TaskId";

ALTER TABLE "TaskCategories" DROP CONSTRAINT "FK_TaskCategories_HelpRequests_TaskId";

ALTER TABLE "TaskOffers" DROP CONSTRAINT "FK_TaskOffers_HelpRequests_TaskId";

ALTER TABLE "HelpRequests" DROP CONSTRAINT "PK_HelpRequests";

ALTER TABLE "HelpRequests" DROP COLUMN "Duration";

ALTER TABLE "HelpRequests" DROP COLUMN "Priority";

ALTER TABLE "HelpRequests" RENAME TO "Tasks";

ALTER INDEX "IX_HelpRequests_CityId" RENAME TO "IX_Tasks_CityId";

ALTER TABLE "Tasks" ADD "DurationOptionId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Tasks" ADD "PriorityOptionId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Tasks" ADD CONSTRAINT "PK_Tasks" PRIMARY KEY ("Id");

CREATE INDEX "IX_Tasks_DurationOptionId" ON "Tasks" ("DurationOptionId");

CREATE INDEX "IX_Tasks_PriorityOptionId" ON "Tasks" ("PriorityOptionId");

ALTER TABLE "Messages" ADD CONSTRAINT "FK_Messages_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE;

ALTER TABLE "TaskCategories" ADD CONSTRAINT "FK_TaskCategories_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE;

ALTER TABLE "TaskOffers" ADD CONSTRAINT "FK_TaskOffers_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE;

ALTER TABLE "Tasks" ADD CONSTRAINT "FK_Tasks_Cities_CityId" FOREIGN KEY ("CityId") REFERENCES "Cities" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Tasks" ADD CONSTRAINT "FK_Tasks_DurationOptions_DurationOptionId" FOREIGN KEY ("DurationOptionId") REFERENCES "DurationOptions" ("Id") ON DELETE CASCADE;

ALTER TABLE "Tasks" ADD CONSTRAINT "FK_Tasks_PriorityOptions_PriorityOptionId" FOREIGN KEY ("PriorityOptionId") REFERENCES "PriorityOptions" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250927173744_SwitchToLookupOptions', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251007151208_SyncUserBio', '9.0.8');


            ALTER TABLE "Users" 
            ADD COLUMN IF NOT EXISTS "Bio" character varying(500);
        

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251007152830_EnsureUserBio', '9.0.8');

COMMIT;

