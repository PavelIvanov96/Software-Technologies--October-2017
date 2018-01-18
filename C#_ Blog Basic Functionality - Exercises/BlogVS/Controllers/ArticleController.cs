using BlogVS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BlogVS.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //Get: List

        public ActionResult List()
        {
            using (var db = new BlogDbContext())
            {
                var articles = db.Articles.Include(a => a.Author).ToList();

                return View(articles);
            }
        }

        //Get:Details

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).FirstOrDefault();

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                return View(article);
            }
        }

        //Get: Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }
        [Authorize]
        //Post: Create
        [HttpPost]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    article.AuthorId = db.Users.Where(u => u.UserName == this.User.Identity.Name).First().Id;

                    db.Articles.Add(article);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        //Get:Delete

        public ActionResult Delete(int? id)

        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).FirstOrDefault();

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                if (CanEdit(article) == false)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                return View(article);
            }
        }

        //Post:Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).FirstOrDefault();

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                if (CanEdit(article) == false)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
        //Get:Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                if (CanEdit(article) == false)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                var model = new ArticleViewModel()
                {
                    Title = article.Title,
                    Content = article.Content,
                    Id = article.Id
                };
                return View(model);
            }    
        }
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var article = db.Articles.FirstOrDefault(x => x.Id == model.Id);
                    article.Title = model.Title;
                    article.Content = model.Content;

                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public bool CanEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}