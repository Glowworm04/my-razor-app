using InternerShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.News
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Models.News> NewsList { get; set; } = new List<Models.News>();

        public async Task OnGetAsync()
        {
            NewsList = await _context.News
                .Include(n => n.Author)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostTogglePublishAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            news.IsPublished = !news.IsPublished;
            news.UpdatedDate = DateTime.UtcNow;

            if (news.IsPublished && news.PublishedDate == default)
            {
                news.PublishedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = news.IsPublished ? "Новость опубликована" : "Новость снята с публикации";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Новость успешно удалена";
            return RedirectToPage();
        }
    }
}
