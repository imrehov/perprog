using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.AccessControl;

namespace futar;

class Program
{
    public static readonly System.Diagnostics.Stopwatch Clock = new();

    static void DrawDashboard(
    Restaurant restaurant,
    List<Courier> couriers)
    {
        var status = restaurant.GetStatus();

        Console.SetCursorPosition(0, 0);

        // Padding erases leftover characters when a new line is shorter.
        Console.WriteLine(
            $"Time: {Clock.Elapsed.TotalSeconds:F1} simulated minutes"
                .PadRight(78));

        string restaurantLine =
            $"Prepared: {status.Prepared}/100 | Waiting: {status.Waiting} | " +
            $"Restaurant: {(status.Finished ? "Closed" : "Open")}";

        Console.WriteLine(restaurantLine.PadRight(78));

        Console.WriteLine();
        Console.WriteLine(
            "FurgeFutar".PadRight(38) + "TurboTeknos".PadRight(38));

        for (int i = 0; i < 4; i++)
        {
            Courier left = couriers[i];
            Courier right = couriers[i + 4];

            Console.WriteLine(
                $"#{left.ID}: {left.State}".PadRight(38) +
                $"#{right.ID}: {right.State}".PadRight(38));
        }

        
    }
    static void Main(string[] args)
    {
        var restaurant = new Restaurant();
        var couriers = new List<Courier>();
        var threads = new List<Thread>();

        

        foreach (Util.Company cmpny in Enum.GetValues<Util.Company>())
        {
            for (int i = 0; i < 4; i++)
            {
                var courier = new Courier(cmpny, restaurant);
                couriers.Add(courier);
                threads.Add(new Thread(courier.Run));
            }
        }

        var restaurantThread = new Thread(restaurant.Run);

        Clock.Start();

        Console.Clear();

        restaurantThread.Start();
        
        foreach (var thread in threads)
        {
            thread.Start();
        }

        while (restaurantThread.IsAlive || threads.Any(t => t.IsAlive))
        {
            DrawDashboard(restaurant, couriers);
            Thread.Sleep(200);
        }

        restaurantThread.Join();

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Clock.Stop();
        DrawDashboard(restaurant, couriers);
        Console.WriteLine();
        Console.WriteLine("Simulation finished!");

        foreach (Util.Company cmpny in Enum.GetValues<Util.Company>())
        {
            decimal total = couriers.Where(c => c.Company == cmpny)
            .Sum(c => c.Earnings);

            System.Console.WriteLine($"{cmpny}: HUF{total}");
        }
    }

    public class Restaurant
    {
        private readonly object gate = new();
        private readonly List<Order> orders = new();
        private bool finished;
        private int prepared;

        public (int Prepared, int Waiting, bool Finished) GetStatus()
        {
            lock (gate)
            {
                return (prepared, orders.Count, finished);
            }
        }

        public void Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(Random.Shared.Next(1000, 5001));
                var order = new Order();

                lock (gate)
                {
                    orders.Add(order);
                    prepared++;
                }
            }

            lock (gate)
            {
                finished = true;
                System.Console.WriteLine("Order production finished!");
            }
        }

        public Order? PickOrder(Util.Company company)
        {
            while (true)
            {
                lock (gate)
                {
                    if (orders.Count > 0)
                    {
                        Order chosen = company == Util.Company.FurgeFutar
                            ? orders.MinBy(o => o.DistanceFromRestaurant)!
                            : orders.MaxBy(o => o.DistanceFromRestaurant)!;

                        orders.Remove(chosen);
                        return chosen;
                    }

                    if (finished)
                        return null;
                }

                // Wait outside the lock so other threads can access the orders.
                Thread.Sleep(Random.Shared.Next(1000, 2001));
            }
        }
    }


    public class Courier
    {
        static int _id = 1;
        public int ID { get; private set; }
        public decimal Earnings { get; private set; }   
        private readonly Restaurant _restaurant;
        public Util.Company Company { get; private set; }

        private volatile Util.State state = Util.State.WaitingForOrder;

        public Util.State State
        {
            get => state;
            private set => state = value;
        }
        

        public Courier(Util.Company cmpny, Restaurant restaurant)
        {
            _restaurant = restaurant;
            Company = cmpny;

            ID = _id++;
        }

        public void Run()
        {
            while (true)
            {
                this.SetState(Util.State.WaitingForOrder);
                Order? order = _restaurant.PickOrder(Company);

                if (order is null)
                {
                    this.SetState(Util.State.Finished);
                    return;
                }

                this.SetState(Util.State.TravelingToCustomer);;
                // Travel to customer.
                double minutesPerKm = Random.Shared.Next(20, 40) / 10.0;
                double travelMinutes =
                    order.DistanceFromRestaurant / 1000.0 * minutesPerKm;

                Thread.Sleep(TimeSpan.FromSeconds(travelMinutes));
                // Hand over order.
                this.SetState(Util.State.HandingOverOrder);
                Thread.Sleep(Random.Shared.Next(2000, 5001));
                // Travel back.

                this.SetState(Util.State.TravelingBack);
                Thread.Sleep(TimeSpan.FromSeconds(travelMinutes));

                // Add payment.
                decimal payment = Company == Util.Company.FurgeFutar
                ? 600m
                : order.Value * 0.05m
                + Math.Ceiling(
                    Math.Max(0, order.DistanceFromRestaurant - 3000) / 1000m
                    ) * 200m;
                
                Earnings += payment;
            }
        }

        private void SetState(Util.State state)
        {
            State = state;
        }


    }

    public class Order
    {
        public int Value { get; private set; }
        public int DistanceFromRestaurant { get; private set; }

        public Order()
        {
            Value = Random.Shared.Next(2000, 10001);
            DistanceFromRestaurant = Random.Shared.Next(500, 10001);
        }
    }

    public static class Util
    {
        public enum Company
        {
            FurgeFutar,
            TurboTeknos
        }
        public enum State
        {
            WaitingForOrder,
            HandingOverOrder,
            TravelingToCustomer,
            TravelingBack,
            Finished,

        }
    }
}
