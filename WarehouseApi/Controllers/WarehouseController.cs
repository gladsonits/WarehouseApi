using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;
        public WarehouseController(IWarehouseRepository warehouseRepository) {
            _warehouseRepository = warehouseRepository;
        }

        //Return OkObjectResult(IEnumerable<WarehouseEntry>)
        [HttpGet("products")]
        public ActionResult<IEnumerable<WarehouseEntry>> GetProducts()
        {
            var products = _warehouseRepository.GetProductRecords(pr => pr.Quantity > 0);
            var warehouseEntries = products.Select(p => new WarehouseEntry { ProductId = p.ProductId, Quantity = p.Quantity });
            return Ok(warehouseEntries);
        }

        [HttpPost("setProductCapacity")]
        public IActionResult SetProductCapacity(int productId, int capacity)
        {
            try
            {
                if (capacity <= 0)
                {
                    throw new WarehouseException(new NotPositiveQuantityMessage());
                }

                var existingCapacity = _warehouseRepository.GetCapacityRecords(cr => cr.ProductId == productId).FirstOrDefault();

                var productDetails = _warehouseRepository.GetProductRecords(pr => pr.ProductId == productId).FirstOrDefault();

                //if the new capacity value is less than the current quantity of the product.
                if (existingCapacity != null && capacity < productDetails?.Quantity)
                {
                    throw new WarehouseException(new QuantityTooLowMessage());
                }

                _warehouseRepository.SetCapacityRecord(productId, capacity);
                return Ok();
            }
            catch (WarehouseException ex)
            {
                return BadRequest(ex.ErrorObject);
            }
        }

        [HttpPost("receiveProduct")]
        public IActionResult ReceiveProduct([FromQuery] int productId, [FromQuery] int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    throw new WarehouseException(new NotPositiveQuantityMessage());
                }

                var capacityRecord = _warehouseRepository.GetCapacityRecords(cr => cr.ProductId == productId).FirstOrDefault();

                if (capacityRecord != null)
                {
                    var currentQuantity = _warehouseRepository.GetProductRecords(pr => pr.ProductId == productId).FirstOrDefault()?.Quantity ?? 0;

                    if (currentQuantity + quantity > capacityRecord.Capacity)
                    {
                        throw new WarehouseException(new QuantityTooHighMessage());
                    }

                    _warehouseRepository.SetProductRecord(productId, currentQuantity + quantity);
                }
                else
                {
                    throw new WarehouseException(new QuantityTooHighMessage());
                }

                return Ok();
            }
            catch (WarehouseException ex)
            {
                return BadRequest(ex.ErrorObject);
            }
        }

        [HttpPost("dispatchProduct")]
        public IActionResult DispatchProduct([FromQuery] int productId, [FromQuery] int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    throw new WarehouseException(new NotPositiveQuantityMessage());
                }

                var productRecord = _warehouseRepository.GetProductRecords(pr => pr.ProductId == productId).FirstOrDefault();

                if (productRecord != null)
                {
                    if (quantity > productRecord.Quantity)
                    {
                        throw new WarehouseException(new QuantityTooHighMessage());
                    }

                    _warehouseRepository.SetProductRecord(productId, productRecord.Quantity - quantity);
                }
                else //if the product pieces not received so far. it is empty. so cannot dispatch product.
                {
                    throw new WarehouseException(new QuantityTooHighMessage());
                }

                return Ok();
            }
            catch (WarehouseException ex)
            {
                return BadRequest(ex.ErrorObject);
            }
        }


    }
}
