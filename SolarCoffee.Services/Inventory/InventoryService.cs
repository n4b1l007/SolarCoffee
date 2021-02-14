using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarCoffee.Data;
using SolarCoffee.Data.Models;

namespace SolarCoffee.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly SolarDbContext _db;
        private readonly ILogger _logger;

        public InventoryService(SolarDbContext db, ILogger<InventoryService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public List<ProductInventory> GetCurrentInventory()
        {
            return _db.ProductInventories
                .Include(pi => pi.Product)
                .Where(pi => !pi.Product.IsArchived)
                .ToList();
        }

        public ServiceResponse<ProductInventory> UpdateUnitsAvailable(int id, int adjustment)
        {
            try
            {
                var inventory = _db.ProductInventories
                    .Include(i => i.Product)
                    .First(i => i.Product.Id == id);
                inventory.QuantityOnHand += adjustment;
                try
                {
                    CreateSnapshot();
                }
                catch (Exception e)
                {
                    _logger.LogError("Error creating inventory snapshot");
                    _logger.LogError(e.StackTrace);
                }
                _db.SaveChanges();

                return new ServiceResponse<ProductInventory>
                {
                    IsSuccess = true,
                    Time = DateTime.UtcNow,
                    Data = inventory,
                    Message = $"Product {id} inventory adjusted",
                };
            }
            catch
            {
                return new ServiceResponse<ProductInventory>
                {
                    IsSuccess = false,
                    Time = DateTime.UtcNow,
                    Data = null,
                    Message = "Error updating ProductInventory QuantityOnHand",
                };
            }
        }

        public ProductInventory GetByProductId(int productId)
        {
            return _db.ProductInventories
            .Include(pi => pi.Product)
            .FirstOrDefault(pi => pi.Product.Id == productId);
        }

        public List<ProductInventorySnapshot> GetSnapshotHistory()
        {
            var earliest = DateTime.UtcNow - TimeSpan.FromHours(3);
            return _db.ProductInventorySnapshots
            .Include(snap => snap.Product)
            .Where(snap => snap.SnapshotTime > earliest
                    && !snap.Product.IsArchived)
            .ToList();
        }

        private void CreateSnapshot()
        {
            var now = DateTime.Now;

            var inventories = _db.ProductInventories
                .Include(inv => inv.Product)
                .ToList();

            foreach (var inventory in inventories)
            {
                var snapshot = new ProductInventorySnapshot
                {
                    SnapshotTime = now,
                    Product = inventory.Product,
                    QuantityOnHand = inventory.QuantityOnHand
                };
                _db.Add(snapshot);

            }
        }
    }
}