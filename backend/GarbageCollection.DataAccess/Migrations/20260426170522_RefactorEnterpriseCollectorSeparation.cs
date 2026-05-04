using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEnterpriseCollectorSeparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Idempotent cleanup for partially-applied migration ───────────────
            // Many steps below already ran; use IF EXISTS / DO $$ so re-running is safe.

            migrationBuilder.Sql(@"
                -- Drop FKs on collector_hub (may already be dropped)
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_hub'
                          AND constraint_name='FK_collector_hub_work_areas_work_area_id'
                    ) THEN
                        ALTER TABLE collector_hub DROP CONSTRAINT ""FK_collector_hub_work_areas_work_area_id"";
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_hub'
                          AND constraint_name='fk_collector_hub_enterprise_id'
                    ) THEN
                        ALTER TABLE collector_hub DROP CONSTRAINT fk_collector_hub_enterprise_id;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='teams'
                          AND constraint_name='FK_teams_collector_hub_collector_id'
                    ) THEN
                        ALTER TABLE teams DROP CONSTRAINT ""FK_teams_collector_hub_collector_id"";
                    END IF;
                END $$;

                -- Drop staffs if still exists
                DROP TABLE IF EXISTS staffs;

                -- Drop assigned_capacity from collector_hub if still exists
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name='collector_hub' AND column_name='assigned_capacity'
                    ) THEN
                        ALTER TABLE collector_hub DROP COLUMN assigned_capacity;
                    END IF;
                END $$;

                -- Rename collector_hub → collectors (if not already done)
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname='public' AND tablename='collector_hub') THEN
                        ALTER TABLE collector_hub RENAME TO collectors;
                    END IF;
                END $$;

                -- Rename teams.collector_id → collector_hub_id (if not already done)
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name='teams' AND column_name='collector_id'
                    ) THEN
                        ALTER TABLE teams RENAME COLUMN collector_id TO collector_hub_id;
                    END IF;
                END $$;

                -- Rename indexes (if still have old names)
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname='IX_teams_collector_id') THEN
                        ALTER INDEX ""IX_teams_collector_id"" RENAME TO ""IX_teams_collector_hub_id"";
                    END IF;
                END $$;
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname='IX_collector_hub_work_area_id') THEN
                        ALTER INDEX ""IX_collector_hub_work_area_id"" RENAME TO ""IX_collectors_work_area_id"";
                    END IF;
                END $$;
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname='IX_collector_hub_enterprise_id') THEN
                        ALTER INDEX ""IX_collector_hub_enterprise_id"" RENAME TO ""IX_collectors_enterprise_id"";
                    END IF;
                END $$;
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname='IX_collector_hub_email') THEN
                        ALTER INDEX ""IX_collector_hub_email"" RENAME TO ""IX_collectors_email"";
                    END IF;
                END $$;

                -- Add PK on collectors if not exists
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collectors' AND constraint_type='PRIMARY KEY'
                    ) THEN
                        ALTER TABLE collectors ADD CONSTRAINT ""PK_collectors"" PRIMARY KEY (id);
                    END IF;
                END $$;

                -- Create collector_hubs if not exists
                CREATE TABLE IF NOT EXISTS collector_hubs (
                    id uuid NOT NULL,
                    name character varying(256) NOT NULL,
                    address character varying(512) NOT NULL,
                    latitude numeric(9,6),
                    longitude numeric(9,6),
                    work_area_id uuid,
                    assigned_capacity integer,
                    collector_id uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_collector_hubs"" PRIMARY KEY (id)
                );

                -- Create enterprise_collector_hubs if not exists
                CREATE TABLE IF NOT EXISTS enterprise_collector_hubs (
                    id uuid NOT NULL,
                    name character varying(256) NOT NULL,
                    phone_number character varying(20) NOT NULL,
                    email character varying(320) NOT NULL,
                    address character varying(512) NOT NULL,
                    latitude numeric(9,6),
                    longitude numeric(9,6),
                    work_area_id uuid,
                    assigned_capacity integer,
                    enterprise_id uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_enterprise_collector_hubs"" PRIMARY KEY (id)
                );

                -- Create collector_staffs if not exists
                CREATE TABLE IF NOT EXISTS collector_staffs (
                    user_id uuid NOT NULL,
                    collector_id uuid NOT NULL,
                    collector_hub_id uuid,
                    team_id uuid,
                    join_team_at timestamp with time zone,
                    CONSTRAINT ""PK_collector_staffs"" PRIMARY KEY (user_id)
                );

                -- Create enterprise_staffs if not exists
                CREATE TABLE IF NOT EXISTS enterprise_staffs (
                    user_id uuid NOT NULL,
                    enterprise_id uuid NOT NULL,
                    enterprise_hub_id uuid,
                    team_id uuid,
                    join_team_at timestamp with time zone,
                    CONSTRAINT ""PK_enterprise_staffs"" PRIMARY KEY (user_id)
                );

                -- ── Indexes (IF NOT EXISTS) ─────────────────────────────────────
                CREATE INDEX IF NOT EXISTS ""IX_collector_hubs_collector_id"" ON collector_hubs (collector_id);
                CREATE INDEX IF NOT EXISTS ""IX_collector_hubs_work_area_id"" ON collector_hubs (work_area_id);
                CREATE INDEX IF NOT EXISTS ""IX_collector_staffs_collector_hub_id"" ON collector_staffs (collector_hub_id);
                CREATE INDEX IF NOT EXISTS ""IX_collector_staffs_collector_id"" ON collector_staffs (collector_id);
                CREATE INDEX IF NOT EXISTS ""IX_collector_staffs_team_id"" ON collector_staffs (team_id);
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_enterprise_collector_hubs_email"" ON enterprise_collector_hubs (email);
                CREATE INDEX IF NOT EXISTS ""IX_enterprise_collector_hubs_enterprise_id"" ON enterprise_collector_hubs (enterprise_id);
                CREATE INDEX IF NOT EXISTS ""IX_enterprise_collector_hubs_work_area_id"" ON enterprise_collector_hubs (work_area_id);
                CREATE INDEX IF NOT EXISTS ""IX_enterprise_staffs_enterprise_hub_id"" ON enterprise_staffs (enterprise_hub_id);
                CREATE INDEX IF NOT EXISTS ""IX_enterprise_staffs_enterprise_id"" ON enterprise_staffs (enterprise_id);
                CREATE INDEX IF NOT EXISTS ""IX_enterprise_staffs_team_id"" ON enterprise_staffs (team_id);

                -- ── FKs on collectors ───────────────────────────────────────────
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collectors'
                          AND constraint_name='FK_collectors_enterprise_hub_enterprise_id'
                    ) THEN
                        ALTER TABLE collectors ADD CONSTRAINT ""FK_collectors_enterprise_hub_enterprise_id""
                            FOREIGN KEY (enterprise_id) REFERENCES enterprise_hub (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collectors'
                          AND constraint_name='FK_collectors_work_areas_work_area_id'
                    ) THEN
                        ALTER TABLE collectors ADD CONSTRAINT ""FK_collectors_work_areas_work_area_id""
                            FOREIGN KEY (work_area_id) REFERENCES work_areas (id) ON DELETE SET NULL;
                    END IF;
                END $$;

                -- ── FKs on collector_hubs ───────────────────────────────────────
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_hubs'
                          AND constraint_name='FK_collector_hubs_collectors_collector_id'
                    ) THEN
                        ALTER TABLE collector_hubs ADD CONSTRAINT ""FK_collector_hubs_collectors_collector_id""
                            FOREIGN KEY (collector_id) REFERENCES collectors (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_hubs'
                          AND constraint_name='FK_collector_hubs_work_areas_work_area_id'
                    ) THEN
                        ALTER TABLE collector_hubs ADD CONSTRAINT ""FK_collector_hubs_work_areas_work_area_id""
                            FOREIGN KEY (work_area_id) REFERENCES work_areas (id) ON DELETE SET NULL;
                    END IF;
                END $$;

                -- ── FKs on enterprise_collector_hubs ────────────────────────────
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_collector_hubs'
                          AND constraint_name='FK_enterprise_collector_hubs_enterprise_hub_enterprise_id'
                    ) THEN
                        ALTER TABLE enterprise_collector_hubs ADD CONSTRAINT ""FK_enterprise_collector_hubs_enterprise_hub_enterprise_id""
                            FOREIGN KEY (enterprise_id) REFERENCES enterprise_hub (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_collector_hubs'
                          AND constraint_name='FK_enterprise_collector_hubs_work_areas_work_area_id'
                    ) THEN
                        ALTER TABLE enterprise_collector_hubs ADD CONSTRAINT ""FK_enterprise_collector_hubs_work_areas_work_area_id""
                            FOREIGN KEY (work_area_id) REFERENCES work_areas (id) ON DELETE SET NULL;
                    END IF;
                END $$;

                -- ── FKs on collector_staffs ─────────────────────────────────────
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_staffs'
                          AND constraint_name='FK_collector_staffs_users_user_id'
                    ) THEN
                        ALTER TABLE collector_staffs ADD CONSTRAINT ""FK_collector_staffs_users_user_id""
                            FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_staffs'
                          AND constraint_name='FK_collector_staffs_collectors_collector_id'
                    ) THEN
                        ALTER TABLE collector_staffs ADD CONSTRAINT ""FK_collector_staffs_collectors_collector_id""
                            FOREIGN KEY (collector_id) REFERENCES collectors (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_staffs'
                          AND constraint_name='FK_collector_staffs_collector_hubs_collector_hub_id'
                    ) THEN
                        ALTER TABLE collector_staffs ADD CONSTRAINT ""FK_collector_staffs_collector_hubs_collector_hub_id""
                            FOREIGN KEY (collector_hub_id) REFERENCES collector_hubs (id) ON DELETE SET NULL;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='collector_staffs'
                          AND constraint_name='FK_collector_staffs_teams_team_id'
                    ) THEN
                        ALTER TABLE collector_staffs ADD CONSTRAINT ""FK_collector_staffs_teams_team_id""
                            FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                -- ── FKs on enterprise_staffs ────────────────────────────────────
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_staffs'
                          AND constraint_name='FK_enterprise_staffs_users_user_id'
                    ) THEN
                        ALTER TABLE enterprise_staffs ADD CONSTRAINT ""FK_enterprise_staffs_users_user_id""
                            FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_staffs'
                          AND constraint_name='FK_enterprise_staffs_enterprise_hub_enterprise_id'
                    ) THEN
                        ALTER TABLE enterprise_staffs ADD CONSTRAINT ""FK_enterprise_staffs_enterprise_hub_enterprise_id""
                            FOREIGN KEY (enterprise_id) REFERENCES enterprise_hub (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_staffs'
                          AND constraint_name='FK_enterprise_staffs_enterprise_collector_hubs_enterprise_hub_~'
                    ) THEN
                        ALTER TABLE enterprise_staffs ADD CONSTRAINT ""FK_enterprise_staffs_enterprise_collector_hubs_enterprise_hub_~""
                            FOREIGN KEY (enterprise_hub_id) REFERENCES enterprise_collector_hubs (id) ON DELETE SET NULL;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='enterprise_staffs'
                          AND constraint_name='FK_enterprise_staffs_teams_team_id'
                    ) THEN
                        ALTER TABLE enterprise_staffs ADD CONSTRAINT ""FK_enterprise_staffs_teams_team_id""
                            FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE RESTRICT;
                    END IF;
                END $$;

                -- ── Fix teams FK (the step that failed) ─────────────────────────
                -- Must clear orphaned team data first (teams.collector_hub_id points to
                -- old collectors rows, not collector_hubs which is now empty)
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name='teams'
                          AND constraint_name='FK_teams_collector_hubs_collector_hub_id'
                    ) THEN
                        -- FK already exists, nothing to do
                        RETURN;
                    END IF;

                    -- Clear dependent data so teams can be deleted safely
                    DELETE FROM team_sessions;
                    UPDATE citizen_reports SET team_id = NULL;
                    DELETE FROM teams;

                    ALTER TABLE teams ADD CONSTRAINT ""FK_teams_collector_hubs_collector_hub_id""
                        FOREIGN KEY (collector_hub_id) REFERENCES collector_hubs (id) ON DELETE RESTRICT;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE teams DROP CONSTRAINT IF EXISTS ""FK_teams_collector_hubs_collector_hub_id"";
                DROP TABLE IF EXISTS collector_staffs;
                DROP TABLE IF EXISTS enterprise_staffs;
                DROP TABLE IF EXISTS collector_hubs;
                DROP TABLE IF EXISTS enterprise_collector_hubs;

                -- Restore staffs table
                CREATE TABLE IF NOT EXISTS staffs (
                    user_id uuid NOT NULL,
                    enterprise_id uuid NOT NULL,
                    collector_id uuid,
                    team_id uuid,
                    join_team_at timestamp with time zone,
                    CONSTRAINT ""PK_staffs"" PRIMARY KEY (user_id)
                );
            ");
        }
    }
}
