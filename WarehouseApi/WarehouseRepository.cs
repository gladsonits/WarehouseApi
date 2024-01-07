using System;
using System.Collections.Generic;
using System.Linq;
using WarehouseApi;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly Dictionary<int, CapacityRecord> _capacityRecords;
    private readonly Dictionary<int, ProductRecord> _productRecords;

    public WarehouseRepository()
    {
        _capacityRecords = new Dictionary<int, CapacityRecord>();
        _productRecords = new Dictionary<int, ProductRecord>();
    }

    public void SetCapacityRecord(int productId, int capacity)
    {
        _capacityRecords[productId] = new CapacityRecord { ProductId = productId, Capacity = capacity };
    }

    public IEnumerable<CapacityRecord> GetCapacityRecords()
    {
        return _capacityRecords.Values;
    }

    public IEnumerable<CapacityRecord> GetCapacityRecords(Func<CapacityRecord, bool> filter)
    {
        return _capacityRecords.Values.Where(filter);
    }

    public void SetProductRecord(int productId, int quantity)
    {
        _productRecords[productId] = new ProductRecord { ProductId = productId, Quantity = quantity };
    }

    public IEnumerable<ProductRecord> GetProductRecords()
    {
        return _productRecords.Values;
    }

    public IEnumerable<ProductRecord> GetProductRecords(Func<ProductRecord, bool> filter)
    {
        return _productRecords.Values.Where(filter);
    }
}


