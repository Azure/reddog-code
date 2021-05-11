create user reddog with password = [PASSWORD];
go

grant create table to reddog;
go

grant control on schema::dbo to reddog;
go


Running migrations
DAPR_GRPC_PORT=5801 dotnet ef migrations add InitialCreate