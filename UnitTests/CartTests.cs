using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Entities;
using System.Linq;
using System.Collections.Generic;

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
    }
}
