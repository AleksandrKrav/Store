using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Domain.Abstract;
using System.Collections.Generic;
using Domain.Entities;
using WebUI.Controllers;
using System.Linq;
using System.Web.Mvc;
using WebUI.Models;
using WebUI.HtmlHelpers;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1"},
                new Book{BookId = 2, Name = "booki2"},
                new Book{BookId = 3, Name = "bookid3"},
                new Book{BookId = 4, Name = "bookid4"},
                new Book{BookId = 5, Name = "bookid5"}
            });

            BooksController controller = new BooksController(mock.Object);
            controller.pageSize = 3;

            BooksListViewModel result = (BooksListViewModel)controller.List(null, 2).Model;

            List<Book> books = result.Books.ToList();

            Assert.IsTrue(books.Count == 2);
            Assert.AreEqual(books[0].Name, "bookid4");
            Assert.AreEqual(books[1].Name, "bookid5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            HtmlHelper myHelper = null;
            PagingInfo paginInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            MvcHtmlString result = myHelper.PageLinks(paginInfo, pageUrlDelegate);

            // Утверждение
            Assert.AreEqual(@"<a class=""btn btn-default"" href=""Page1"">1</a>"
                + @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>"
                + @"<a class=""btn btn-default"" href=""Page3"">3</a>",
                result.ToString());
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1"},
                new Book{BookId = 2, Name = "booki2"},
                new Book{BookId = 3, Name = "bookid3"},
                new Book{BookId = 4, Name = "bookid4"},
                new Book{BookId = 5, Name = "bookid5"}
            });

            BooksController controller = new BooksController(mock.Object);
            controller.pageSize = 3;

            BooksListViewModel result = (BooksListViewModel)controller.List(null, 2).Model;
            PagingInfo pagingInfo = result.PagingInfo;
            Assert.AreEqual(pagingInfo.CurrentPage, 2);
            Assert.AreEqual(pagingInfo.ItemsPerPage, 3);
            Assert.AreEqual(pagingInfo.TotalItems, 5);
            Assert.AreEqual(pagingInfo.TotalPages, 2);

        }

        [TestMethod]
        public void Can_Filter_Books()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1", Genre = "Genre1"},
                new Book{BookId = 2, Name = "booki2", Genre = "Genre2"},
                new Book{BookId = 3, Name = "bookid3", Genre = "Genre3"},
                new Book{BookId = 4, Name = "bookid4", Genre = "Genre4"},
                new Book{BookId = 5, Name = "bookid5", Genre = "Genre5"}
            });

            BooksController controller = new BooksController(mock.Object);
            controller.pageSize = 3;

            List<Book> result = ((BooksListViewModel)controller.List("Genre2", 2).Model).Books.ToList();

            Assert.AreEqual(result.Count(), 2);
            Assert.IsTrue(result[0].Name == "bookid2" && result[0].Genre == "Genre2");
            Assert.IsTrue(result[1].Name == "bookid5" && result[1].Genre == "Genre4");

        }


        [TestMethod]
        public void Can_Create_Categories()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1", Genre = "Genre1"},
                new Book{BookId = 2, Name = "booki2", Genre = "Genre2"},
                new Book{BookId = 3, Name = "bookid3", Genre = "Genre1"},
                new Book{BookId = 4, Name = "bookid4", Genre = "Genre3"},
                new Book{BookId = 5, Name = "bookid5", Genre = "Genre2"}
            });

            NavController target = new NavController(mock.Object);

            List<string> result = ((IEnumerable<string>)target.Menu().Model).ToList();

            Assert.AreEqual(result.Count(), 3);
            Assert.AreEqual(result[0], "Genre1");
            Assert.AreEqual(result[1], "Genre2");
            Assert.AreEqual(result[2], "Genre3");         
        }

        [TestMethod]
        public void Can_Selected_Genre()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1", Genre = "Genre1"},
                new Book{BookId = 2, Name = "booki2", Genre = "Genre2"},
                new Book{BookId = 3, Name = "bookid3", Genre = "Genre1"},
                new Book{BookId = 4, Name = "bookid4", Genre = "Genre3"},
                new Book{BookId = 5, Name = "bookid5", Genre = "Genre2"}
            });

            NavController target = new NavController(mock.Object);

            string genreToSelected = "Genre2";

            string result = target.Menu(genreToSelected).ViewBag.SelectedGenre;

                Assert.AreEqual(genreToSelected, result );
        }

        [TestMethod]
        public void Genrerate_Genre_Specific_Book_Count()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>
            {
                new Book{BookId = 1, Name = "bookid1", Genre = "Genre1"},
                new Book{BookId = 2, Name = "booki2", Genre = "Genre2"},
                new Book{BookId = 3, Name = "bookid3", Genre = "Genre1"},
                new Book{BookId = 4, Name = "bookid4", Genre = "Genre3"},
                new Book{BookId = 5, Name = "bookid5", Genre = "Genre2"}
            });

            BooksController controller = new BooksController(mock.Object);
            controller.pageSize = 3;

            int res1 = ((BooksListViewModel)controller.List("Genre1").Model).PagingInfo.TotalItems;
            int res2 = ((BooksListViewModel)controller.List("Genre2").Model).PagingInfo.TotalItems;
            int res3 = ((BooksListViewModel)controller.List("Genre3").Model).PagingInfo.TotalItems;
            int resAll = ((BooksListViewModel)controller.List(null).Model).PagingInfo.TotalItems;

            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
            Assert.AreEqual(resAll, 5);
        }

       
    }
}
