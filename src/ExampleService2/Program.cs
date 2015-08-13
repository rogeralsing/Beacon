using System.Threading;
using Nancy;
using Shared;

namespace CustomerService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Host.StartServices("customers");
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/customers/{customerid}"] = _ =>
            {
                var customers = new[]
                {
                    new Customer
                    {
                        Id = 123,
                        Name = "Acme Inc"
                    },
                    new Customer
                    {
                        Id = 456,
                        Name = "Lazer Sharks"
                    }
                };

                return Response.AsJson(customers);
            };

            Get["/customers/{customerid}"] = _ =>
            {
                var customer = new Customer
                {
                    Id = 123,
                    Name = "Acme Inc"
                };

                return Response.AsJson(customer);
            };
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}