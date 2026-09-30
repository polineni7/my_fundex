from pathlib import Path
from uuid import uuid5,NAMESPACE_URL
root=Path('database');root.mkdir(exist_ok=True)
lines=['-- Optional demonstration catalogue. Plans remain DRAFT until reviewed and published.','BEGIN;']
def uid(name): return str(uuid5(NAMESPACE_URL,'myfundex/'+name))
def insert(schema,table,fields,values,condition):
 cols=','.join('"'+x+'"' for x in fields+['CreatedAt','CreatedBy','IsDeleted','Version'])
 lines.append(f'INSERT INTO {schema}."{table}" ({cols}) SELECT '+','.join(values+['CURRENT_TIMESTAMP','0','false','1'])+f' WHERE NOT EXISTS (SELECT 1 FROM {schema}."{table}" WHERE {condition});')
for table,idcol,code,name in [('Currencies','CurrencyId','INR','Indian Rupee'),('Countries','CountryId','IN','India'),('Exchanges','ExchangeId','NSE','National Stock Exchange'),('SecurityTypes','SecurityTypeId','EQUITY','Equity')]:
 fields=[idcol,'Code','Name'];values=[f"'{uid(code)}'",f"'{code}'",f"'{name}'"]
 if table=='Currencies':fields+=['DecimalPlaces'];values+=['2']
 insert('fundex_master',table,fields,values,f'"Code"=\'{code}\'')
for role,name in [('ADMIN','Administrator'),('MANAGER','Manager'),('TRADER','Trader')]:
 insert('fundex_identity','Roles',['RoleId','Code','Name'],[f"'{uid(role)}'",f"'{role}'",f"'{name}'"],f'"Code"=\'{role}\'')
for area in ['plans','accounts','policies','settings','audit','orders','positions','withdrawals','payments','users','master']:
 for action in ['read','write']:
  code=area+'.'+action
  insert('fundex_identity','Permissions',['PermissionId','Code','Name'],[f"'{uid(code)}'",f"'{code}'",f"'{area} {action}'"],f'"Code"=\'{code}\'')
for count in [2,3]:
 code=f'INR-100K-{count}STEP';plan=uid(code);version=uid(code+'-v1')
 insert('fundex_subscription','Plans',['PlanId','Code','Name','Description'],[f"'{plan}'",f"'{code}'",f"'100K {count}-Step Evaluation'","'Demonstration draft. Review all fees and risk settings before publication.'"],f'"PlanId"=\'{plan}\'')
 insert('fundex_subscription','PlanVersions',['PlanVersionId','PlanInternalId','VersionNumber','ChallengeCapital','RegistrationFee','Status','EffectiveFrom','Path','RewardSharePercent'],[f"'{version}'",f'(SELECT "Id" FROM fundex_subscription."Plans" WHERE "PlanId"=\'{plan}\')','1','100000','999',"'Draft'",'CURRENT_TIMESTAMP',"'TwoStep'" if count==2 else "'ThreeStep'",'80'],f'"PlanVersionId"=\'{version}\'')
 for stage in range(1,count+1):
  stageid=uid(code+f'-stage{stage}')
  insert('fundex_subscription','Stages',['StageId','PlanVersionInternalId','StageNumber','Name','StartingCapital','PolicySetId','RequiredForCompletion','ProfitTargetPercent','MaxDailyLossPercent','MaxTotalLossPercent','MinimumTradingDays'],[f"'{stageid}'",f'(SELECT "Id" FROM fundex_subscription."PlanVersions" WHERE "PlanVersionId"=\'{version}\')',str(stage),f"'Stage {stage}'",'100000',"'00000000-0000-0000-0000-000000000000'",'true','8' if stage==1 else '5','5','10','5'],f'"StageId"=\'{stageid}\'')
lines+=['COMMIT;','-- Stage PolicySetId must be assigned to an approved active policy before publishing.','-- Administrators are created with Bootstrap:AdminEmail/Bootstrap:AdminPassword; never insert plaintext passwords.']
(root/'002_demo_catalogue.sql').write_text('\n\n'.join(lines)+'\n')
