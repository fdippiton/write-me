using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WriteMe_MVC.Controllers
{
    public class FavoritosController : Controller
    {
        // GET: FavoritosController
        public ActionResult Index()
        {
            return View();
        }

        // GET: FavoritosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FavoritosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FavoritosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FavoritosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FavoritosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FavoritosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FavoritosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
