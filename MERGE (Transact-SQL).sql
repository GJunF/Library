https://docs.microsoft.com/zh-cn/previous-versions/sql/sql-server-2008-r2/bb510625(v=sql.105)

MERGE Production.UnitMeasure AS target
    USING
        ( SELECT    @UnitMeasureCode ,
                    @Name
        ) AS source ( UnitMeasureCode, Name )
    ON ( target.UnitMeasureCode = source.UnitMeasureCode )
    WHEN MATCHED THEN
        UPDATE SET
               Name = source.Name
    WHEN NOT MATCHED THEN
        INSERT ( UnitMeasureCode, Name )
        VALUES ( source.UnitMeasureCode ,
                 source.Name
               )
    OUTPUT
        deleted.* ,
        $action ,
        inserted.*
        INTO #MyTempTable;
        
        
        
MERGE Production.ProductInventory AS target
USING
    ( SELECT    ProductID ,
                SUM(OrderQty)
      FROM      Sales.SalesOrderDetail AS sod
                JOIN Sales.SalesOrderHeader AS soh ON sod.SalesOrderID = soh.SalesOrderID
                                                      AND soh.OrderDate = @OrderDate
      GROUP BY  ProductID
    ) AS source ( ProductID, OrderQty )
ON ( target.ProductID = source.ProductID )
WHEN MATCHED AND target.Quantity - source.OrderQty <= 0 THEN
    DELETE
WHEN MATCHED THEN
    UPDATE SET target.Quantity = target.Quantity - source.OrderQty ,
               target.ModifiedDate = GETDATE()
OUTPUT
    $action ,
    Inserted.ProductID ,
    Inserted.Quantity ,
    Inserted.ModifiedDate ,
    Deleted.ProductID ,
    Deleted.Quantity ,
    Deleted.ModifiedDate;
    
    
MERGE INTO Sales.SalesReason AS Target
USING ( VALUES
    ( 'Recommendation' ,'Other' ),
    ( 'Review' ,'Marketing' ),
    ( 'Internet' ,'Promotion' ) ) AS Source ( NewName, NewReasonType )
ON Target.Name = Source.NewName
WHEN MATCHED THEN
    UPDATE SET ReasonType = Source.NewReasonType
WHEN NOT MATCHED BY TARGET THEN
    INSERT ( Name, ReasonType )
    VALUES ( NewName ,
             NewReasonType
           )
OUTPUT
    $action
    INTO @SummaryOfChanges;