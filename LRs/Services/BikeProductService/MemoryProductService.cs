using Microsoft.AspNetCore.Mvc;

namespace LRs.Services.BikeProductService
{
    public class MemoryProductService : IBikeProductService
    {
        private readonly IConfiguration _config;
        private readonly int _maxPageSize = 20;

        private List<Bike> _bikes;
        private List<Category> _categories;

        public MemoryProductService(
            [FromServices] IConfiguration config,
            IBikeCategoryService categoryService)
        {
            _config = config;

            _categories = categoryService.GetCategoryListAsync()
                .Result
                .Data;

            SetupData();
        }

        Task<ResponseData<Bike>> IBikeProductService.CreateProductAsync(
            Bike product,
            IFormFile? formFile)
        {
            throw new NotImplementedException();
        }

        Task IBikeProductService.DeleteProductAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<ResponseData<Bike>> IBikeProductService.GetProductByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        async Task<ResponseData<ListModel<Bike>>> IBikeProductService.GetProductListAsync(
            string? categoryNormalizedName,
            int pageNo = 1)
        {
            var result = new ListModel<Bike>();

            int pageSize = _config.GetValue<int>("PageSize");

            IEnumerable<Bike> query = _bikes;

            if (!string.IsNullOrEmpty(categoryNormalizedName))
            {
                query = query.Where(b =>
                    b.Category != null &&
                    b.Category.NormalizedName == categoryNormalizedName);
            }

            int totalItems = query.Count();

            result.TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);
            if (result.TotalPages == 0)
                result.TotalPages = 1;

            if (pageNo < 1)
                pageNo = 1;

            if (pageNo > result.TotalPages)
                pageNo = result.TotalPages;

            result.CurrentPage = pageNo;

            result.Items = query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new ResponseData<ListModel<Bike>>
            {
                Data = result,
                Successfull = true
            };
        }

        /// <summary>
        /// Инициализация списков
        /// </summary>
        private void SetupData()
        {
            _bikes = new List<Bike>
            {
                new Bike
                {
                    Id = 1,
                    Name = "Шоссейник Aero",
                    Description = "Лёгкая алюминиевая рама и узкие покрышки для быстрой езды по асфальту",
                    Price = 1600,
                    Image = "../Images/road.jpg",
                    MimeType = "image/jpeg",
                    Category = _categories.Find(c =>
                        c.NormalizedName.Equals("road"))
                },

                new Bike
                {
                    Id = 2,
                    Name = "Грэвел Adventure",
                    Description = "Универсал: асфальт, грунт, лёгкое бездорожье",
                    Price = 4400,
                    Image = "../Images/gravel.jpg",
                    MimeType = "image/jpeg",
                    Category = _categories.Find(c =>
                        c.NormalizedName.Equals("gravel"))
                },

                new Bike
                {
                    Id = 3,
                    Name = "Горный Trail Pro",
                    Description = "Амортизационная вилка, широкие покрышки",
                    Price = 1000,
                    Image = "../Images/mountain.jpg",
                    MimeType = "image/jpeg",
                    Category = _categories.Find(c =>
                        c.NormalizedName.Equals("mountain"))
                },

                new Bike
                {
                    Id = 4,
                    Name = "Фикс SingleSpeed",
                    Description = "Одна передача, минимализм, стиль города",
                    Price = 1300,
                    Image = "../Images/fixie.jpg",
                    MimeType = "image/jpeg",
                    Category = _categories.Find(c =>
                        c.NormalizedName.Equals("fixie"))
                },

                new Bike
                {
                    Id = 5,
                    Name = "Гибрид City",
                    Description = "Комфорт для города и лёгких прогулок",
                    Price = 700,
                    Image = "../Images/hybrid.jpg",
                    MimeType = "image/jpeg",
                    Category = _categories.Find(c =>
                        c.NormalizedName.Equals("hybrid"))
                }
            };
        }

        Task IBikeProductService.UpdateProductAsync(
            int id,
            Bike product,
            IFormFile? formFile)
        {
            throw new NotImplementedException();
        }
    }
}