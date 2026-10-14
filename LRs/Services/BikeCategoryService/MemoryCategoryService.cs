namespace LRs.Services.BikeCategoryService
{
    public class MemoryCategoryService : IBikeCategoryService
    {
        public Task<ResponseData<List<Category>>> GetCategoryListAsync()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Шоссейный", NormalizedName = "road" },
                new Category { Id = 2, Name = "Грэвел",     NormalizedName = "gravel" },
                new Category { Id = 3, Name = "Горный",     NormalizedName = "mountain" },
                new Category { Id = 4, Name = "Фикс",       NormalizedName = "fixie" },
                new Category { Id = 5, Name = "Гибрид",     NormalizedName = "hybrid" }
            };
            var result = ResponseData<List<Category>>.Success(categories);
            return Task.FromResult(result);
        }
    }
}
