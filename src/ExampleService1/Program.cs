using System.Collections.Generic;
using Nancy;
using Shared;

namespace OrderService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Host.StartServices("orders");
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/orders"] = _ =>
            {
                return "hello";
            };
            Get["/orders/{orderid}"] = _ =>
            {
                var order = new Order()
                {
                    OrderId = 897,
                    CustomerId = 123,
                    Details = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            ProductId = 555,
                            Quantity = 2
                        },
                        new OrderDetail()
                        {
                            ProductId = 222,
                            Quantity = 987
                        }
                    }
                };

                return Response.AsJson(order);
            };
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public List<OrderDetail> Details { get; set; }
    }

    public class OrderDetail
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
    }
}