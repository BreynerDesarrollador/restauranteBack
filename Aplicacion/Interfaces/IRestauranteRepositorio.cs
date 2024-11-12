using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Interfaces
{
    public interface IRestauranteRepositorio : IbaseRepositorio<Restaurante>
    {
        Task<IEnumerable<Restaurante>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Restaurante>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
        Task<IEnumerable<Restaurante>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<bool> UpdateStockAsync(string id, int quantity, CancellationToken cancellationToken = default);
    }
}
