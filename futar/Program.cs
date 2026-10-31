using System.Collections.Concurrent;
using System.Net;
using System.Security.AccessControl;

namespace futar;

class Program
{
    static void Main(string[] args)
    {
        
    }

    public class Restaurant
    {
        public string State { get; set; }
        public static List<Order> Orders { get; private set; }

        public Restaurant()
        {
            State = "Open";
            PrepareNewOrder();
        }

        public void PrepareNewOrder()
        {
            Thread.Sleep(Util.rnd.Next(1000, 5001));
            Orders.Add(new Order());
        }
    }


    public class Courier
    {
        public Util.Company Company { get; private set; }

        public Courier(Util.Company cmpny)
        {
            Company = cmpny;
        }

        public void DeliverOrder()
        {
            PickOrder();
            Thread.Sleep(Util.rnd.Next(2000, 3901));
        }

        private void PickOrder()
        {
            Thread.Sleep(Util.rnd.Next(2000, 5001));
            if (this.Company == Util.Company.FurgeFutar)
            {
                PickOrderFurge();
            }
            PickOrderTeknos();
        }

        private void PickOrderFurge()
        {
            while (true)
            {
                var closest = Restaurant.Orders.OrderBy(t => t.DistanceFromRestaurant)
                .FirstOrDefault();

                if (closest == null)
                {
                    Thread.Sleep(Util.rnd.Next(1, 2001));
                }
                else
                {
                    Restaurant.Orders.Remove(closest);
                    break;
                }
            }       
        }

        private void PickOrderTeknos()
        {
            while (true)
            {
                var furthest = Restaurant.Orders.OrderByDescending(t => t.DistanceFromRestaurant)
                .FirstOrDefault();

                if (furthest == null)
                {
                    Thread.Sleep(Util.rnd.Next(1, 2001));
                }
                else
                {
                    Restaurant.Orders.Remove(furthest);
                    break;
                }
            }
        }


    }

    public class Order
    {
        public int Value { get; private set; }
        public int DistanceFromRestaurant { get; private set; }

        public Order()
        {
            Value = Util.rnd.Next(2000, 10001);
            DistanceFromRestaurant = Util.rnd.Next(500, 10001);
        }
    }

    public static class Util
    {
        public static Random rnd = new Random();

        public enum Company
        {
            FurgeFutar,
            TurboTeknos
        }
    }
}
