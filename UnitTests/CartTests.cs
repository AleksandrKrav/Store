using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Domain.Abstract;
using Moq;
using WebUI.Controllers;
using System.Web.Mvc;
using WebUI.Models;

namespace UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };

            Cart cart = new Cart();
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 2);

            List<CartLine> results = cart.Lines.ToList();

            Assert.AreEqual(results.Count(), 2);
            Assert.AreEqual(results[0].Book, book1);
            Assert.AreEqual(results[1].Book, book2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };

            Cart cart = new Cart();
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 2);
            cart.AddItem(book1, 5);

            List<CartLine> results = cart.Lines.OrderBy(c => c.Book.BookId).ToList();

            Assert.AreEqual(results.Count(), 2);
            Assert.AreEqual(results[0].Quantity, 6);
            Assert.AreEqual(results[1].Quantity, 2);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };
            Book book3 = new Book { BookId = 3, Name = "Book3" };

            Cart cart = new Cart();

            cart.AddItem(book1, 1);
            cart.AddItem(book2, 2);
            cart.AddItem(book1, 5);
            cart.AddItem(book3, 2);

            cart.RemoveLine(book3);



            Assert.AreEqual(cart.Lines.Where(h => h.Book == book3).Count(), 0);
            Assert.AreEqual(cart.Lines.Count(), 2);
        
        }

        [TestMethod]
        public void Can_Cart_Total()
        {
            Book book1 = new Book { BookId = 1, Name = "Book1", Price = 100};
            Book book2 = new Book { BookId = 2, Name = "Book2", Price = 300 };
            Book book3 = new Book { BookId = 3, Name = "Book3" , Price = 20};

            Cart cart = new Cart();

            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 1);
            cart.AddItem(book3, 1);

            decimal res = cart.ComputeTotalValue();
            Assert.AreEqual(res, 520);
        }

        [TestMethod]
        public void Can_Clear_Cart()
        {
            Book book1 = new Book { BookId = 1, Name = "Book1", Price = 100 };
            Book book2 = new Book { BookId = 2, Name = "Book2", Price = 300 };
            Book book3 = new Book { BookId = 3, Name = "Book3", Price = 20 };

            Cart cart = new Cart();

            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 1);
            cart.AddItem(book3, 1);

            cart.Clear();

            Assert.AreEqual(cart.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>{
                new Book {BookId = 1, Name = "Book1", Genre = "Genre1"},
                new Book {BookId = 2, Name = "Book2", Genre = "Genre2"},
                new Book {BookId = 3, Name = "Book3", Genre = "Genre3"}
            }.AsQueryable());

            Cart cart = new Cart();

            CartController controller = new CartController(mock.Object, null);

            controller.AddToCart(cart, 1, null);

            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToList()[0].Book.BookId, 1);

        }

        // После добавления книги в корзину - перенаправление на страницу корзины
        [TestMethod]
        public void Adding_Book_To_Cart_Goes_To_Cart_Screen()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>{
                new Book {BookId = 1, Name = "Book1", Genre = "Genre1"}
            }.AsQueryable());

            Cart cart = new Cart();

            CartController controller = new CartController(mock.Object, null);

            RedirectToRouteResult result = controller.AddToCart(cart, 2, "myUrl");

            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            Cart cart = new Cart();
            CartController target = new CartController(null, null);

            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            ShippingDetails shippingDetails = new ShippingDetails();

            CartController controller = new CartController(null, mock.Object);

            ViewResult result = controller.Checkout(cart, shippingDetails);

            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Book(), 1);

            CartController controller = new CartController(null, mock.Object);
            controller.ModelState.AddModelError("error", "error");

            ViewResult result = controller.Checkout(cart, new ShippingDetails());

            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_And_Submit_Order()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Book(), 1);

            CartController controller = new CartController(null, mock.Object);

            ViewResult result = controller.Checkout(cart, new ShippingDetails());

            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());

            Assert.AreEqual("Completed", result.ViewName);
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }
    }
}
