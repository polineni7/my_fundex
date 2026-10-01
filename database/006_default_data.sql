-- Optional demonstration catalogue. Plans remain DRAFT until reviewed and published.

BEGIN;
SELECT pg_advisory_xact_lock(20261001, 6);

INSERT INTO fundex_master."Currencies" ("CurrencyId","Code","Name","DecimalPlaces","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '836d8a91-9a91-5789-9dbf-301d032f021d','INR','Indian Rupee',2,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_master."Currencies" WHERE "Code"='INR');

INSERT INTO fundex_master."Countries" ("CountryId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '8234ccb7-a7e6-5da1-858f-fd44e49f41aa','IN','India',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_master."Countries" WHERE "Code"='IN');

INSERT INTO fundex_master."Exchanges" ("ExchangeId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '21f205f1-b22d-525a-b2a0-0bd20c297ba0','NSE','National Stock Exchange',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_master."Exchanges" WHERE "Code"='NSE');

INSERT INTO fundex_master."SecurityTypes" ("SecurityTypeId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '1b7a0578-dc62-5e8c-b1ed-28160df13b50','EQUITY','Equity',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_master."SecurityTypes" WHERE "Code"='EQUITY');

INSERT INTO fundex_identity."Roles" ("RoleId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '36c393dc-f2a4-51a5-9249-40e33392c8cb','ADMIN','Administrator',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Roles" WHERE "Code"='ADMIN');

INSERT INTO fundex_identity."Roles" ("RoleId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'fbd23054-a615-5d60-9554-0c8fe9407113','MANAGER','Manager',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Roles" WHERE "Code"='MANAGER');

INSERT INTO fundex_identity."Roles" ("RoleId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '529a0ecb-3823-5f86-9320-597f55dc9ba6','TRADER','Trader',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Roles" WHERE "Code"='TRADER');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '7802642c-688f-5b51-b256-3e5769323a79','plans.read','plans read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='plans.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'fb48690e-8103-52a6-9258-3ab562879901','plans.write','plans write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='plans.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '131a17ba-8fc5-5dfa-acba-d004d8642787','accounts.read','accounts read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='accounts.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'da4b2410-78c6-5f63-abc4-4eb0d386be29','accounts.write','accounts write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='accounts.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '6f508459-33db-51ea-a6ce-3b665e5ef771','policies.read','policies read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='policies.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '5d22e998-31e2-5300-abb7-104e4fe8a7eb','policies.write','policies write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='policies.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '94c29a88-0806-58a3-b525-dbc46522fb55','settings.read','settings read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='settings.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'e975f867-da33-5b62-a13d-b1a9a2995f26','settings.write','settings write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='settings.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'b4bedd00-2473-5477-8303-ec007d741dca','audit.read','audit read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='audit.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '0730c996-0e3b-578e-9e06-acd5222e4134','audit.write','audit write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='audit.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'e166ad70-60e2-54d9-a6a6-ede16f2851f1','orders.read','orders read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='orders.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'd999248e-2f6e-5063-94e0-417d72e11fad','orders.write','orders write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='orders.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '19334e09-6998-5daa-96ef-390313ed660a','positions.read','positions read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='positions.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '92746154-4708-5c73-be2e-a4a1248a3584','positions.write','positions write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='positions.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '6efa8d5d-fe20-5b7d-9941-36b7342793f6','withdrawals.read','withdrawals read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='withdrawals.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '022b5cc5-a7ff-57a6-bfd7-d25c4ffa2c26','withdrawals.write','withdrawals write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='withdrawals.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '5a4e279c-ccf6-5af8-bd97-4d48c8894f31','payments.read','payments read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='payments.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '7293a615-49c3-5789-9c8c-529944f979f1','payments.write','payments write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='payments.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'ba34003e-9c27-523f-bd16-89e715446453','users.read','users read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='users.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '319ebe1d-01fc-5411-9dde-ce45e2d4e130','users.write','users write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='users.write');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '880d367d-7a5a-55fe-b746-c6a76aa34b49','master.read','master read',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='master.read');

INSERT INTO fundex_identity."Permissions" ("PermissionId","Code","Name","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'db362c64-3ba8-59ae-bb0f-a7b8d502db2c','master.write','master write',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Permissions" WHERE "Code"='master.write');

INSERT INTO fundex_subscription."Plans" ("PlanId","Code","Name","Description","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'c2c969e5-1ba3-56cc-a5be-7f463a10f292','INR-100K-2STEP','100K 2-Step Evaluation','Demonstration draft. Review all fees and risk settings before publication.',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Plans" WHERE "PlanId"='c2c969e5-1ba3-56cc-a5be-7f463a10f292');

INSERT INTO fundex_subscription."PlanVersions" ("PlanVersionId","PlanInternalId","VersionNumber","ChallengeCapital","AssessmentFee","Status","EffectiveFrom","Path","RewardSharePercent","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318',(SELECT "Id" FROM fundex_subscription."Plans" WHERE "PlanId"='c2c969e5-1ba3-56cc-a5be-7f463a10f292'),1,100000,999,'Draft',CURRENT_TIMESTAMP,'TwoStep',80,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318');

INSERT INTO fundex_subscription."Stages" ("StageId","PlanVersionInternalId","StageNumber","Name","StartingCapital","PolicySetId","RequiredForCompletion","ProfitTargetPercent","MaxDailyLossPercent","MaxTotalLossPercent","MinimumTradingDays","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'b89eb6ea-aae9-5683-98ac-d0b5004dfa3f',(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318'),1,'Stage 1',100000,'00000000-0000-0000-0000-000000000000',true,8,5,10,5,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Stages" WHERE "StageId"='b89eb6ea-aae9-5683-98ac-d0b5004dfa3f');

INSERT INTO fundex_subscription."Stages" ("StageId","PlanVersionInternalId","StageNumber","Name","StartingCapital","PolicySetId","RequiredForCompletion","ProfitTargetPercent","MaxDailyLossPercent","MaxTotalLossPercent","MinimumTradingDays","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '8dc51ab7-ab47-562b-a9dd-9161fb0883f1',(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318'),2,'Stage 2',100000,'00000000-0000-0000-0000-000000000000',true,5,5,10,5,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Stages" WHERE "StageId"='8dc51ab7-ab47-562b-a9dd-9161fb0883f1');

INSERT INTO fundex_subscription."Plans" ("PlanId","Code","Name","Description","CreatedAt","CreatedBy","IsDeleted","Version") SELECT '1beb332e-b148-56a8-9efa-a3a08ac8a57a','INR-100K-3STEP','100K 3-Step Evaluation','Demonstration draft. Review all fees and risk settings before publication.',CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Plans" WHERE "PlanId"='1beb332e-b148-56a8-9efa-a3a08ac8a57a');

INSERT INTO fundex_subscription."PlanVersions" ("PlanVersionId","PlanInternalId","VersionNumber","ChallengeCapital","AssessmentFee","Status","EffectiveFrom","Path","RewardSharePercent","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'f6779897-6557-5179-8c1d-8461806bd3ca',(SELECT "Id" FROM fundex_subscription."Plans" WHERE "PlanId"='1beb332e-b148-56a8-9efa-a3a08ac8a57a'),1,100000,999,'Draft',CURRENT_TIMESTAMP,'ThreeStep',80,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='f6779897-6557-5179-8c1d-8461806bd3ca');

INSERT INTO fundex_subscription."Stages" ("StageId","PlanVersionInternalId","StageNumber","Name","StartingCapital","PolicySetId","RequiredForCompletion","ProfitTargetPercent","MaxDailyLossPercent","MaxTotalLossPercent","MinimumTradingDays","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'be419ded-86f7-50ed-82b5-f8f36d8674da',(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='f6779897-6557-5179-8c1d-8461806bd3ca'),1,'Stage 1',100000,'00000000-0000-0000-0000-000000000000',true,8,5,10,5,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Stages" WHERE "StageId"='be419ded-86f7-50ed-82b5-f8f36d8674da');

INSERT INTO fundex_subscription."Stages" ("StageId","PlanVersionInternalId","StageNumber","Name","StartingCapital","PolicySetId","RequiredForCompletion","ProfitTargetPercent","MaxDailyLossPercent","MaxTotalLossPercent","MinimumTradingDays","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'f92da84b-7aab-52ee-93df-963c9012923f',(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='f6779897-6557-5179-8c1d-8461806bd3ca'),2,'Stage 2',100000,'00000000-0000-0000-0000-000000000000',true,5,5,10,5,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Stages" WHERE "StageId"='f92da84b-7aab-52ee-93df-963c9012923f');

INSERT INTO fundex_subscription."Stages" ("StageId","PlanVersionInternalId","StageNumber","Name","StartingCapital","PolicySetId","RequiredForCompletion","ProfitTargetPercent","MaxDailyLossPercent","MaxTotalLossPercent","MinimumTradingDays","CreatedAt","CreatedBy","IsDeleted","Version") SELECT 'f3f19870-585e-5cd8-a37a-85fc7c52925b',(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"='f6779897-6557-5179-8c1d-8461806bd3ca'),3,'Stage 3',100000,'00000000-0000-0000-0000-000000000000',true,5,5,10,5,CURRENT_TIMESTAMP,0,false,1 WHERE NOT EXISTS (SELECT 1 FROM fundex_subscription."Stages" WHERE "StageId"='f3f19870-585e-5cd8-a37a-85fc7c52925b');


-- Baseline permissions are additive and do not replace existing assignments.
INSERT INTO fundex_identity."RolePermissions" ("RoleInternalId","PermissionInternalId","CreatedAt","CreatedBy","IsDeleted","Version")
SELECT r."Id", p."Id", now(), 0, false, 1
FROM fundex_identity."Roles" r CROSS JOIN fundex_identity."Permissions" p
WHERE NOT r."IsDeleted" AND NOT p."IsDeleted"
AND (r."Code" = 'ADMIN'
 OR (r."Code" = 'MANAGER' AND p."Code" IN ('plans.read','plans.write','accounts.read','policies.read','policies.write','audit.read','orders.read','positions.read','master.read'))
 OR (r."Code" = 'TRADER' AND p."Code" IN ('plans.read','accounts.read','orders.read','orders.write','positions.read','withdrawals.read','withdrawals.write','payments.read')))
AND NOT EXISTS (SELECT 1 FROM fundex_identity."RolePermissions" rp WHERE rp."RoleInternalId"=r."Id" AND rp."PermissionInternalId"=p."Id");

INSERT INTO fundex_risk."PolicySets" ("PolicyId","Code","Name","PolicyType","CreatedAt","CreatedBy","IsDeleted","Version")
SELECT 'bc44b1e5-a3b3-5026-83b2-d936e4e0138f', 'DEFAULT-EQUITY-100K', 'Default equity evaluation policy', 'Trading', now(),0,false,1
WHERE NOT EXISTS (SELECT 1 FROM fundex_risk."PolicySets" WHERE "Code"='DEFAULT-EQUITY-100K' OR "PolicyId"='bc44b1e5-a3b3-5026-83b2-d936e4e0138f');
INSERT INTO fundex_risk."PolicyVersions" ("PolicyVersionId","PolicySetInternalId","VersionNumber","Status","EffectiveFrom","ApplicationMode","CreatedAt","CreatedBy","IsDeleted","Version")
SELECT '7402d4da-701f-5bd0-b5ac-0d2f011826a1', "Id", 1,'Active',now(),'NextTrade',now(),0,false,1 FROM fundex_risk."PolicySets"
WHERE "PolicyId"='bc44b1e5-a3b3-5026-83b2-d936e4e0138f' AND NOT "IsDeleted"
AND NOT EXISTS (SELECT 1 FROM fundex_risk."PolicyVersions" WHERE "PolicySetInternalId"=fundex_risk."PolicySets"."Id" AND "VersionNumber"=1);
INSERT INTO fundex_risk."Rules" ("RuleId","PolicyVersionInternalId","RuleCode","Name","Category","Operator","ViolationAction","Priority","IsEnabled","DecimalValue","CreatedAt","CreatedBy","IsDeleted","Version")
SELECT 'dfa48469-7273-5e42-b05f-06ae7af9f8e1',"Id",'EQUITY_ONLY','EQUITY_ONLY','Trading','GreaterThan','RejectOrder',1,true,NULL,now(),0,false,1 FROM fundex_risk."PolicyVersions"
WHERE "PolicyVersionId"='7402d4da-701f-5bd0-b5ac-0d2f011826a1' AND NOT "IsDeleted"
AND NOT EXISTS (SELECT 1 FROM fundex_risk."Rules" WHERE "RuleId"='dfa48469-7273-5e42-b05f-06ae7af9f8e1' OR ("PolicyVersionInternalId"=fundex_risk."PolicyVersions"."Id" AND "RuleCode"='EQUITY_ONLY'));
INSERT INTO fundex_risk."Rules" ("RuleId","PolicyVersionInternalId","RuleCode","Name","Category","Operator","ViolationAction","Priority","IsEnabled","DecimalValue","CreatedAt","CreatedBy","IsDeleted","Version")
SELECT '9d4cd4b7-ec0c-53c8-bdc0-13c5a6e709f2',"Id",'MAX_ORDER_VALUE','MAX_ORDER_VALUE','Trading','GreaterThan','RejectOrder',2,true,100000,now(),0,false,1 FROM fundex_risk."PolicyVersions"
WHERE "PolicyVersionId"='7402d4da-701f-5bd0-b5ac-0d2f011826a1' AND NOT "IsDeleted"
AND NOT EXISTS (SELECT 1 FROM fundex_risk."Rules" WHERE "RuleId"='9d4cd4b7-ec0c-53c8-bdc0-13c5a6e709f2' OR ("PolicyVersionInternalId"=fundex_risk."PolicyVersions"."Id" AND "RuleCode"='MAX_ORDER_VALUE'));
-- Only fill unassigned policies on the unchanged demonstration drafts.
UPDATE fundex_subscription."Stages" s SET "PolicySetId"='bc44b1e5-a3b3-5026-83b2-d936e4e0138f',"UpdatedAt"=now(),"UpdatedBy"=0,"Version"=s."Version"+1
FROM fundex_subscription."PlanVersions" v
WHERE s."PlanVersionInternalId"=v."Id" AND v."Status"='Draft' AND NOT v."IsDeleted" AND NOT s."IsDeleted"
AND v."PlanVersionId" IN ('8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318','f6779897-6557-5179-8c1d-8461806bd3ca')
AND s."PolicySetId"='00000000-0000-0000-0000-000000000000'
AND EXISTS (SELECT 1 FROM fundex_risk."PolicySets" WHERE "PolicyId"='bc44b1e5-a3b3-5026-83b2-d936e4e0138f' AND NOT "IsDeleted");
COMMIT;
