use ShippingComp

create table Clients
(
Id int identity primary key,
Name nvarchar(50),
Address nvarchar(50),
Phone nvarchar(20),
)

create table Shipments
(
Id int identity primary key,
TrackingNumber nvarchar(50),
ShipmentDate Date,
Weight decimal,
ClientId int foreign key references Clients(Id)
)

create table Invoices
(
Id int identity primary key,
BasePrice DECIMAL,
Tax DECIMAL,
Total DECIMAL,
CreatedAt DATETIME,
ShipmentId int foreign key references Shipments(Id),
)

create table Payments
(
Id int identity primary key,
InvoiceId int foreign key references Invoices(Id),
AmountPaid decimal,
PaymentDate Date	
)

alter table Payments add constraint DF_PaymentDate Default GetDate() for PaymentDate
---------------------------------------------------------------------------------------------------
INSERT INTO Clients (Name, Address, Phone)
VALUES 
('Global Freight Co.', 'Alexandria', '01012345678'),
('Nile Shipping', 'Cairo', '01087654321');

INSERT INTO Shipments (TrackingNumber, ShipmentDate, Weight, ClientId)
VALUES
('TRK001', '2025-02-01', 120.5, 1),
('TRK002', '2025-02-05', 98.3, 2);

INSERT INTO Invoices (ShipmentId, BasePrice, Tax, Total)
VALUES
(1, 1500, 150, 1650),
(2, 2000, 200, 2200);

INSERT INTO Payments (InvoiceId, AmountPaid, PaymentDate)
VALUES
(1, 1000, '2025-02-10'),
(2, 2200, '2025-02-15');

---------------------------------------------------------------------------------------------------
--make client name qnique
alter table Clients add constraint UniqueName unique (name)


--a function (inline function) that returns all invoices for certain client:
create function GetInvoicesForClient(@clientName nvarchar(50))
returns table
as return
(
select i.* from Invoices i
join Shipments s on i.ShipmentId = s.Id
join Clients c on c.Id = s.ClientId
where c.Name = @clientName
)

select * from GetInvoicesForClient('Global Freight Co.')

select * from GetInvoicesForClient('abc')


--an SP returns that inserts invoice and some other data:
--test:
create proc sp_GetShipmentsForClient 
@cname nvarchar(50)
@shipments table(ShipmentId int, TrackingNumber nvarchar(50)) output
as
begin
	insert into @shipments
	select s.id,s.TrackingNumber 
	from Shipments s join Invoices i on i.ShipmentId = s.Id 
	join Clients c on c.Id = s.ClientId
	where c.Name = @cname
end

create function GetShipmentsForClient(@cid int)
returns @shipments table(ShipmentId int, TrackingNumber nvarchar(50))
as
begin
	select s.id,s.TrackingNumber 
	from Shipments s join Invoices i on i.ShipmentId = s.Id 
	join Clients c on c.Id = s.ClientId
	where c.Id = @cid
	return
end


create function GetShipmentsForClient(@cid int)
returns table
as return
(
	select s.id,s.TrackingNumber 
	from Shipments s join Invoices i on i.ShipmentId = s.Id 
	join Clients c on c.Id = s.ClientId
	where c.Id = @cid
)

select * from GetShipmentsForClient(2)

--------------------------------------------------------------------------------------
--function gets payments per client
alter function GetPaymentsForClient(@client varchar(50))
returns table 
as return
(
	select c.Name 'Client', s.TrackingNumber 'TrackingNumber', i.Id 'Invoice Id', i.Total, p.Id 'Payment Id', p.AmountPaid
	from Clients c 
	join Shipments s on s.ClientId = c.Id
	join Invoices i on i.ShipmentId = s.Id
	join Payments p on p.InvoiceId = i.Id
	where c.Name = @client
)

select * from GetPaymentsForClient('Global Freight Co.')

create function GetAllPaymentsForAllClients()
returns table 
as return
(
	select c.Name 'Client', s.TrackingNumber 'TrackingNumber', i.Id 'Invoice Id', i.Total, p.Id 'Payment Id', p.AmountPaid
	from Clients c 
	join Shipments s on s.ClientId = c.Id
	join Invoices i on i.ShipmentId = s.Id
	join Payments p on p.InvoiceId = i.Id
)


select * from GetAllPaymentsForAllClients()


alter function GetPaymentsForClientV2(@client varchar(50) = 'none')
returns @ClientPayments 
table (Client nvarchar(50), TrackingNumber nvarchar(50), Invoice_Id int, Total decimal(20,2), Payment_Id int, Amount_Paid decimal(20,2))
as
begin
	if(@client != 'none')
		begin
			insert  @ClientPayments
			select c.Name 'Client', s.TrackingNumber, i.Id 'Invoice Id', i.Total, p.Id 'Payment Id', p.AmountPaid
			from Clients c 
			join Shipments s on s.ClientId = c.Id
			join Invoices i on i.ShipmentId = s.Id
			join Payments p on p.InvoiceId = i.Id
			where c.Name = @client
		end
	else
		begin
			insert @ClientPayments
			select * from GetPaymentsForClient()
		end
	return
end

select * from GetPaymentsForClientV2('Global Freight Co.');

select * from dbo.GetPaymentsForClientV2();


create function GetPaymentsForClientV3(@client varchar(50) = null)
returns @ClientPayments table
(Client nvarchar(50), TrackingNumber nvarchar(50), Invoice_Id int, Total decimal(20,2), Payment_Id int, Amount_Paid decimal(20,2))
as
begin
	insert @ClientPayments
	select c.Name 'Client', s.TrackingNumber, i.Id 'Invoice Id', i.Total, p.Id 'Payment Id', p.AmountPaid
	from Clients c 
	join Shipments s on s.ClientId = c.Id
	join Invoices i on i.ShipmentId = s.Id
	join Payments p on p.InvoiceId = i.Id
	where c.Name = @client or @client = null
	return
end

select * from GetPaymentsForClientV3();
drop function GetPaymentsForClientV3
drop function GetPaymentsForClientV2
--------------------------------------------------
--add status column in payments table
alter table Invoices add Status varchar(7) check (Status in ('Paid','Partial','Unpaid'))
ALTER TABLE Invoices ADD CONSTRAINT DF_StatusUnPaid DEFAULT 'Unpaid' FOR Status;


alter table payments drop column status
alter table payments drop CK__Payments__Status__151B244E

alter table Invoices drop CK__Invoices__Status__2B0A656D
go
alter table Invoices drop DF_StatusUnPaid
go
alter table Invoices drop column status
--------------------------------------------------
alter VIEW InvoicesAndAmountPaid
AS
SELECT
    i.Id 'Invoice Id',
	c.name 'Client Name',
	i.Total 'Total Cost',
	i.ShipmentId,
    isnull(sum(p.AmountPaid), 0) as 'Total Amount Paid',
    CASE
        WHEN isnull(sum(p.AmountPaid), 0) = 0 THEN 'Unpaid'
        WHEN sum(p.AmountPaid) < i.Total THEN 'Partial'
        WHEN sum(p.AmountPaid) = i.Total THEN 'Paid'
        WHEN sum(p.AmountPaid) > i.Total THEN 'Over Paid'
    END AS Status,
	i.Total - isnull(sum(p.AmountPaid), 0) 'Amount to pay'
FROM Invoices i
left outer JOIN Payments p ON i.Id = p.InvoiceId
join Shipments s on s.id = i.ShipmentId
join Clients c on c.Id = s.ClientId
group by i.Id, c.name ,i.Total ,i.ShipmentId

select * from InvoicesAndAmountPaid


select distinct InvoiceId from Payments
 
select * from
(select *, row_number() over(partition by invoiceid order by invoiceid) as gg
from Payments) as h
where gg = 1

select * , Dense_RANK() over(order by invoiceid)
from Payments


--------------------------
-- make total in Invoices table derived column
alter table Invoices drop column total
alter table Invoices add Total as (BasePrice + Tax)

-- Retrieved make current date and time the default for any invoice if not otherwise
ALTER TABLE Invoices ADD CONSTRAINT DF_CreatedAt DEFAULT GetDate() FOR CreatedAt;

select * from Invoices
---------------------------------------
--simulate payment operation 
--after client (id: 1044, name: elgohary corp) has made a shipment (id: 9, tracking num: XYZ100)
--and supposedly invoice (id: 1005, total: 1524) is auto generated for that shipment
--the client now is making a payment
	--insert into payments(InvoiceId, AmountPaid) values (1005, 500) -- payment id: 3
	--insert into payments(InvoiceId, AmountPaid) values (1005, 1000) -- payment id: 4
	--insert into payments(InvoiceId, AmountPaid) values (1005, 500) -- payment id: 5
	--insert into payments(InvoiceId, AmountPaid) values (1005, 24) -- payment id: 6

-- another payment for another shipment 
	--insert into payments(InvoiceId, AmountPaid) values (1006, 200) -- payment id: 7
	--insert into payments(InvoiceId, AmountPaid) values (1006, 132) -- payment id: 8
--

select * from Payments

select * from InvoicesAndAmountPaid

------------------------------------------------------
--function to filter payments by client 
create function FilterPaymentsByClient(@client_name nvarchar(50))
RETURNS TABLE
as
return (
	select * from InvoicesAndAmountPaid
	where [Client Name] = @client_name
)


select * from FilterPaymentsByClient('elgohary corp')

------------------------------------------------------
--trigger to add invoice after a shipment has been made by a client X
--stored to add invoice after a shipment has been made by a client using theis sp OK
--test try catch
declare @x int
set @x = 1
begin try
	if (@x = 2)
		THROW 99999, 'A custom error: cannot choose (1)', 1;
	select 'gg'
end try
begin catch
	select ERROR_MESSAGE() 
end catch


-- make it use theo sp_AddShipment, which auto create TrackingNumber
alter proc sp_AddShipmentAndInvoice @weight decimal(18, 0), @clientid int, @shipmentdate date = null
as
begin
	if @shipmentdate is null set @shipmentdate = SYSUTCDATETIME();
	declare @baseprice decimal = (3.89*@weight+145)
	declare @tax decimal = @baseprice*0.14
	declare @shipmentid int
	begin try
		begin transaction
		--insert into Shipments values(@trackingnumber, @shipmentdate,@weight, @clientid)
		execute sp_AddShipment @weight, @clientid, @shipmentdate, @shipmentid output
		--set @shipmentid = (select id from Shipments where TrackingNumber = @trackingnumber)
		--set @shipmentid = scope_identity()
		insert into Invoices values(@baseprice,@tax,@shipmentdate,@shipmentid)
		commit transaction
	end try
	begin catch
		rollback
		select ERROR_MESSAGE() 
	end catch
end

sp_AddShipmentAndInvoice 10512, 2044, '12/25/2016'


-- make tracking number unique
alter table shipments
add constraint UNQ_trackingnumber_shipments unique (trackingnumber)

------------------------------------------------------
--prevent over payment for an invoice
--making a stored procedure to insert through
create procedure ValidateAndPerformPayment @invoiceid int, @amountpaid decimal, @paymentdate date
as
begin
	insert into Payments values (@invoiceid, @amountpaid, @paymentdate)
end

--logic:
/*
if sum(amount paid) for invoice id > total return 0 from the sp and do not perform the insert 
*/

--(select sum(amountpaid) from payments where InvoiceId = @invoiceid)

--1 check if invoice is fully paid => do not allow any payment at all
--2 if its unpaid/partial accept only the <= invoice.total and if there is more credit return it to the client

--1:
declare @array table (invoiceid int, status varchar(10), clientid int)
insert into @array --insert based on select (insert the result of a select query in some other table)
select [Invoice Id], Status, s.ClientId from InvoicesAndAmountPaid
join Shipments s on s.Id = ShipmentId
where [Invoice Id] = 5

select status from @array

------------------------------------------------------
-- 4 jan 2026
--some enhancements on Shipments table
--ALTER TABLE shipments ALTER COLUMN clientid INT NOT NULL
--ALTER TABLE shipments ADD DEFAULT SYSUTCDATETIME() FOR ShipmentDate;
ALTER TABLE shipments ALTER COLUMN TrackingNumber nvarchar(50) NOT NULL
ALTER TABLE shipments drop UNQ_trackingnumber_shipments
-- make tracking number unique
alter table shipments add constraint UNQ_trackingnumber_shipments unique (trackingnumber)


-- SP to add shipment, with auto generate TrackingNumber

alter proc sp_AddShipment @w dec, @cId int, @date date, @shipmentid int output
as
begin
	insert into Shipments(ShipmentDate,Weight,ClientId,TrackingNumber) values (@date,@w,@cId,0)
	--declare @shipmentId int = scope_identity()
	--select @shipmentid_Output = @shipmentId
	select @shipmentid = scope_identity();
	declare @trkNum nvarchar(50) = concat('TRK',@shipmentId,'-',@w)
	update Shipments set TrackingNumber = @trkNum where id = @shipmentId
end

declare @gg int
execute sp_AddShipment 959, 1, '3/13/2004', @gg output
select @gg

-- #now use this sp in sp_AddShipmentAndInvoice above ##done





